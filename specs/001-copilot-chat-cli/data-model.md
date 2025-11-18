# Data Model: M365 Copilot Chat CLI

**Date**: 2025-11-15  
**Feature**: 001-copilot-chat-cli

## Entity Definitions

### Conversation

Represents a chat session with M365 Copilot created via API.

**Attributes**:
- `Id` (string, GUID format): Unique identifier for the conversation, assigned by API
- `CreatedDateTime` (DateTime): Timestamp when conversation was created
- `DisplayName` (string): Human-readable name for the conversation (initially empty, updated after first message)
- `Status` (enum: Active, Closed): Current state of the conversation
- `TurnCount` (int): Number of message exchanges in the conversation

**Relationships**:
- Has many Messages (one-to-many)

**Validation Rules**:
- Id must be valid GUID format
- Status can only transition from Active → Closed (not reversible)
- TurnCount increments with each message pair (user message + copilot response)

**State Transitions**:
- Created (status=Active, turnCount=0) → First message sent (status=Active, turnCount=1) → Subsequent messages (turnCount++) → Session end (status unchanged in this release, conversation ID retained for potential future continuation)

**Persistence**: Not persisted locally; conversation ID stored in memory during interactive session only

---

### Message

Represents user input sent to a conversation or Copilot's response.

**Attributes**:
- `Text` (string, required): Message content
- `CreatedDateTime` (DateTime): When message was created
- `MessageType` (enum: UserMessage, CopilotResponse): Distinguishes between user and system messages

**Relationships**:
- Belongs to Conversation (many-to-one)

**Validation Rules**:
- Text cannot be null or empty for user messages
- Text length limited by API (specific limit not documented, handle gracefully if rejected)
- CreatedDateTime must be valid ISO 8601 format when received from API

**Constraints**:
- User messages are send-only (CLI → API)
- Copilot responses are receive-only (API → CLI via SSE)
- Messages are not persisted locally (stateless tool design)

---

### ResponseEvent

Represents a single Server-Sent Event containing response chunks from M365 Copilot.

**Attributes**:
- `EventId` (string, optional): SSE event ID for reconnection scenarios
- `EventType` (string, optional): SSE event type (default: "message")
- `Data` (string, JSON-encoded): Payload containing CopilotConversation object
- `RetryInterval` (int, optional): Milliseconds to wait before reconnecting (SSE spec)

**Structure of Data Payload**:
```json
{
  "id": "conversation-guid",
  "displayName": "string",
  "state": "active",
  "turnCount": int,
  "messages": [
    {
      "@odata.type": "#microsoft.graph.copilotConversationResponseMessage",
      "id": "message-guid",
      "text": "incremental or complete response text",
      "createdDateTime": "ISO 8601",
      "adaptiveCards": [...],  // Optional, UI rendering hints
      "attributions": [...],   // Optional, source citations
      "sensitivityLabel": {...} // Optional, data classification
    }
  ]
}
```

**Relationships**:
- Contains Conversation snapshot (embedded, not relational)
- Contains Messages array (embedded in Data payload)

**Parsing Logic**:
- Each SSE event may contain partial or complete conversation state
- `messages[]` array accumulates over time (incremental updates)
- Final event typically contains complete conversation with all messages
- Extract latest message text from `messages[messages.length - 1].text`

**Constraints**:
- Events arrive in stream order (no reordering needed)
- Not all events contain new message text (some are intermediate state updates)
- Must handle events with empty `messages[]` array gracefully

---

### Authentication Token

Bearer token read from environment variable, used for M365 API authentication.

**Attributes**:
- `Value` (string, required): JWT or opaque token string
- `Source` (string): Environment variable name (e.g., "M365_COPILOT_TOKEN")

**Validation Rules**:
- Value must be non-empty
- Value format not validated locally (API will reject invalid tokens)
- No expiration check (user responsible for token freshness per spec assumptions)

**Usage**:
- Injected in `Authorization: Bearer {Value}` header for all API requests
- Read once at application startup
- Not refreshed during execution (single-session tool)

**Security Considerations**:
- Never logged or written to stdout/stderr
- Not persisted to disk
- User responsible for securing environment variable per spec

---

## Entity Relationship Diagram

```
┌─────────────────────┐
│  AuthToken          │
│  - Value            │
│  - Source           │
└──────────┬──────────┘
           │ (used by)
           ▼
┌─────────────────────┐         ┌─────────────────────┐
│  Conversation       │◄────────│  Message            │
│  - Id               │ 1     * │  - Text             │
│  - CreatedDateTime  │         │  - CreatedDateTime  │
│  - DisplayName      │         │  - MessageType      │
│  - Status           │         └─────────────────────┘
│  - TurnCount        │
└──────────┬──────────┘
           │ (streamed via)
           ▼
┌─────────────────────┐
│  ResponseEvent      │
│  - EventId          │
│  - EventType        │
│  - Data (JSON)      │
│    └─ Conversation  │
│       └─ Messages[] │
└─────────────────────┘
```

## Data Flow

1. **Startup**: AuthToken loaded from environment variable
2. **Create Conversation**: POST /conversations → receives Conversation entity (Id, CreatedDateTime, etc.)
3. **Send Message**: POST /conversations/{Id}/chatOverStream with Message.Text
4. **Receive Response**: SSE stream of ResponseEvent[] → parse Data → extract Message.Text → display incrementally
5. **Interactive Mode**: Repeat steps 3-4 using same Conversation.Id
6. **Shutdown**: Conversation.Id discarded (no local persistence)

## Implementation Notes

- All entities are immutable once received from API (no local mutations)
- Conversation.Id is the only state maintained between interactive turns
- Messages are displayed as received (no buffering required beyond SSE parsing)
- No database or file persistence needed (stateless design per spec)
