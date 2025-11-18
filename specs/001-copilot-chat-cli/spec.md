# Feature Specification: M365 Copilot Chat CLI

**Feature Branch**: `001-copilot-chat-cli`  
**Created**: 2025-11-15  
**Status**: Draft  
**Input**: User description: "Create a command line tool that will call the M365 Copilot Chat API. It will have two modes. A one-shot mode where you pass some text as a parameter to the commandline and that text gets forwarded to M365 Copilot API. And another interactive mode that works like REPL where you can have a multi-turn conversation with M365 Copilot."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - One-Shot Query Mode (Priority: P1)

A user needs to quickly ask M365 Copilot a single question and receive an answer without entering an interactive session. They run the CLI with their question as a command-line argument, the tool creates a conversation, sends the message, streams the response to their terminal, and exits.

**Why this priority**: This is the minimal viable product - a working CLI that can communicate with M365 Copilot API. It demonstrates end-to-end integration (authentication, conversation creation, message sending, response streaming) without the complexity of session management.

**Independent Test**: Can be fully tested by running the CLI with a text argument and verifying the response is streamed from M365 Copilot to the console. Delivers immediate value for scripting and automation scenarios.

**Acceptance Scenarios**:

1. **Given** a valid M365 token in the environment variable, **When** user runs `copilot-cli "What is the weather today?"`, **Then** the tool creates a conversation, sends the message, streams the response to stdout, and exits with code 0
2. **Given** a valid token, **When** user runs the tool with a question, **Then** the response appears progressively as Server-Sent Events are received (not all at once at the end)
3. **Given** no token or an invalid token, **When** user runs the tool, **Then** an error message is displayed to stderr and the tool exits with a non-zero code
4. **Given** network connectivity issues, **When** the tool attempts to call the API, **Then** a clear error message is shown and the tool exits gracefully

---

### User Story 2 - Interactive REPL Mode (Priority: P2)

A user wants to have a multi-turn conversation with M365 Copilot where context is maintained across messages. They run the CLI without arguments to enter interactive mode, type messages at a prompt, receive streaming responses, and continue the conversation until they choose to exit.

**Why this priority**: Builds on P1 by adding session management and multi-turn context. Requires conversation state tracking but reuses the same API integration from P1.

**Independent Test**: Can be tested by launching the CLI without arguments, sending multiple messages, and verifying responses maintain conversational context. Delivers value for exploratory research and complex queries.

**Acceptance Scenarios**:

1. **Given** a valid token, **When** user runs the tool without arguments, **Then** an interactive prompt appears (e.g., `copilot>`)
2. **Given** the interactive prompt is active, **When** user types a message and presses Enter, **Then** the message is sent to the existing conversation and the response is streamed to the console
3. **Given** multiple messages have been sent, **When** user asks a follow-up question referencing previous context, **Then** M365 Copilot maintains conversation history and responds appropriately
4. **Given** the interactive session is running, **When** user types `exit`, `quit`, or presses Ctrl+C, **Then** the session ends gracefully and the tool exits
5. **Given** an error occurs during the session, **When** the error is handled, **Then** the user is returned to the prompt rather than the tool crashing

---

### User Story 3 - Token-Based Authentication (Priority: P3)

A user needs to provide an authentication token to access the M365 Copilot API. They set an environment variable containing a valid bearer token before running the tool. The tool validates the token presence and provides clear error messages when the token is missing or invalid.

**Why this priority**: Essential for any API access but can use simple environment variable approach initially. Interactive login with secure token caching is deferred to a future release due to complexity of secure credential storage and token refresh flows.

**Independent Test**: Can be tested by running the tool with and without the token environment variable set, and verifying appropriate success/failure behavior.

**Acceptance Scenarios**:

1. **Given** a valid M365 token is set in the environment variable, **When** user runs the tool, **Then** the token is used for API authentication
2. **Given** no token environment variable is set, **When** user runs the tool, **Then** a helpful error message explains which environment variable to set and how to obtain a token
3. **Given** an expired or invalid token is provided, **When** the tool attempts an API call, **Then** a clear error message indicates the authentication failure (401/403) with guidance on obtaining a fresh token

---

### Edge Cases

- What happens when the streaming response is interrupted mid-stream (network failure, API timeout)?
- How does the tool handle extremely long responses that exceed typical terminal buffer sizes?
- What happens if the user sends an empty message in interactive mode?
- How does the tool behave when rate limits are hit on the M365 Copilot API?
- What happens if the conversation creation API fails but the tool is in one-shot mode?
- How are special characters and multi-line input handled in both modes?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Tool MUST accept a text string as a command-line argument for one-shot mode
- **FR-002**: Tool MUST enter interactive REPL mode when launched without arguments
- **FR-003**: Tool MUST authenticate with M365 Copilot API using a bearer token read from an environment variable
- **FR-004**: Tool MUST call the conversation creation API (`POST /me/copilot/conversations`) before sending messages
- **FR-005**: Tool MUST send messages using the chat over stream API (`POST /me/copilot/conversations/{id}/chatOverStream`)
- **FR-006**: Tool MUST parse and display Server-Sent Events (SSE) as they arrive in real-time
- **FR-007**: Tool MUST maintain conversation context across multiple messages in interactive mode
- **FR-008**: Tool MUST support graceful exit from interactive mode via `exit`, `quit`, or Ctrl+C
- **FR-009**: Tool MUST write response content to stdout and errors/diagnostics to stderr
- **FR-010**: Tool MUST display clear error messages for authentication failures, network errors, and API errors
- **FR-011**: Tool MUST exit with code 0 on success and non-zero codes for different error conditions
- **FR-012**: Tool MUST handle SSE stream parsing according to the SSE specification (event types, data fields, etc.)

### Key Entities

- **Conversation**: Represents a chat session with M365 Copilot, created via API and identified by an ID, maintains message history and context
- **Message**: User input sent to the conversation, includes text content and metadata
- **Response Event**: Server-Sent Event containing response chunks from M365 Copilot, includes event type and data payload
- **Authentication Token**: Bearer token read from environment variable, used in Authorization header for all M365 API requests

## Assumptions

- **Token Acquisition**: Users are responsible for obtaining a valid M365 bearer token through external means (e.g., Azure Portal, OAuth flow in browser). The tool does not implement interactive login or token refresh in this release.
- **Token Security**: Users must secure the token environment variable appropriately for their environment. Secure token caching is explicitly deferred to a future release.
- **Token Lifetime**: Users must manually refresh tokens when they expire. No automatic token refresh is provided in this release.

## Clarifications

### Session 2025-11-15

- Q: Kiota SDK Integration Approach → A: Use `dotnet tool install kiota` locally (manifest), generate SDK manually before each build
- Q: Model Class Generation Strategy → A: Let Kiota generate all model classes from OpenAPI spec (Conversation, Message, ResponseEvent models)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can execute a one-shot query and receive a complete response in under 5 seconds (excluding API processing time)
- **SC-002**: Response streaming begins within 2 seconds of sending a message
- **SC-003**: Interactive mode supports at least 50 consecutive messages in a single session without degradation
- **SC-004**: Tool handles network interruptions gracefully with clear error messages in 100% of test scenarios
- **SC-005**: Authentication errors are detected and reported with actionable guidance within 3 seconds
- **SC-006**: Users can successfully maintain conversational context across multiple turns (verified by testing follow-up questions)
- **SC-007**: Tool startup time is under 1 second in one-shot mode and under 2 seconds in interactive mode
