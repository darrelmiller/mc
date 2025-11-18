# Research: M365 Copilot Chat CLI

**Date**: 2025-11-15  
**Feature**: 001-copilot-chat-cli  
**Purpose**: Resolve NEEDS CLARIFICATION items from Technical Context

## Research Questions

### 1. SSE Parsing Library for .NET

**Question**: What SSE parsing library should be used for .NET to handle Server-Sent Events from M365 Copilot API?

**Decision**: Use `System.Net.ServerSentEvents.SseParser` (built-in .NET library, available in .NET 9+, backport available for .NET 8)

**Rationale**:
- Official Microsoft library introduced in .NET 9 with backport support for .NET 8 via NuGet
- Purpose-built for parsing SSE streams according to W3C EventSource specification
- Integrates seamlessly with HttpClient for streaming responses
- No external dependencies required
- Example usage from Microsoft documentation:
  ```csharp
  Stream responseStream = await httpClient.GetStreamAsync(uri);
  await foreach (SseItem<string> e in SseParser.Create(responseStream).EnumerateAsync())
  {
      Console.WriteLine(e.Data);
  }
  ```

**Alternatives Considered**:
- **Manual parsing**: Rejected because it's error-prone and violates DRY principles when official library exists
- **Third-party libraries**: Rejected because System.Net.ServerSentEvents is first-party, well-supported, and requires no additional dependencies

**Implementation Notes**:
- Use `HttpCompletionOption.ResponseHeadersRead` to avoid buffering entire response
- Parse SSE events asynchronously using `SseParser.Create(stream).EnumerateAsync()`
- Extract text content from `messages[].text` field in copilotConversation response
- Handle SSE event IDs for potential reconnection scenarios (future enhancement)

---

### 2. OpenAPI Specification for M365 Copilot Chat API

**Question**: Does an OpenAPI specification exist for M365 Copilot Chat API, or do we need to create one for Kiota SDK generation?

**Decision**: Create OpenAPI specification from Microsoft Learn documentation (no official spec published)

**Rationale**:
- Microsoft Learn documentation provides complete API contract details but no downloadable OpenAPI/Swagger file
- Constitutional requirement: must use Kiota-generated SDK (cannot write manual HTTP code)
- Creating spec from documentation ensures accurate contract representation and enables Kiota code generation

**API Contract Details** (from Microsoft Learn docs):

**Endpoint 1: Create Conversation**
- **Method**: `POST`
- **Path**: `/beta/copilot/conversations`
- **Request**: Empty JSON body `{}`
- **Response**: `201 Created` with `copilotConversation` object
- **Response Schema**:
  ```json
  {
    "id": "string (GUID)",
    "createdDateTime": "string (ISO 8601)",
    "displayName": "string",
    "status": "active | closed",
    "turnCount": integer
  }
  ```

**Endpoint 2: Chat Over Stream**
- **Method**: `POST`
- **Path**: `/beta/copilot/conversations/{conversationId}/chatOverStream`
- **Request Body**:
  ```json
  {
    "message": {
      "text": "string (required)"
    },
    "locationHint": {
      "timeZone": "string (required, IANA format)"
    },
    "additionalContext": [ 
      { "text": "string" }
    ] (optional),
    "contextualResources": {
      "files": [ { "uri": "string" } ],
      "webContext": { "isWebEnabled": boolean }
    } (optional)
  }
  ```
- **Response**: `200 OK` with `Content-Type: text/event-stream`
- **SSE Event Format**: Each event contains `copilotConversation` object with `messages[]` array

**Authentication**: Bearer token via `Authorization` header

**Required Permissions** (delegated, work/school account):
- Sites.Read.All
- Mail.Read
- People.Read.All
- OnlineMeetingTranscript.Read.All
- Chat.Read
- ChannelMessage.Read.All
- ExternalItem.Read.All

**Implementation Notes**:
- Create `openapi.yaml` with above contract
- Use Kiota CLI to generate SDK: `kiota generate -l csharp -d openapi.yaml -o Generated/CopilotSdk`
- Configure kiota-config.json for repeatable generation
- Add Generated/ to .gitignore (regenerate at build time)

---

### 3. Testing Strategy: Real API vs Mocks

**Question**: Should integration tests call the real M365 Copilot API or use mocked responses?

**Decision**: Use both approaches with clear separation

**Rationale**:
- **Unit tests**: Mock all API calls to test CLI logic, SSE parsing, error handling in isolation
- **Integration tests (optional/manual)**: Real API calls for end-to-end validation, requires valid M365 credentials
- Separation ensures tests can run in CI/CD without credentials while still enabling real-world validation when needed

**Testing Strategy**:

**Unit Tests (xUnit)**:
- Mock `ICopilotClient` interface for testing OneShotCommand and InteractiveCommand
- Mock SSE stream responses for testing StreamParser
- Test AuthProvider with mock environment variables
- Test error handling and exit codes
- **Run in**: CI/CD pipelines, pre-commit hooks

**Integration Tests (xUnit, tagged/manual)**:
- Real HTTP calls to M365 Copilot API endpoints
- Requires environment variable with valid bearer token
- Tagged with `[Trait("Category", "Integration")]` for selective execution
- Tests end-to-end flow: conversation creation → message send → SSE parsing
- **Run in**: Developer machines with valid credentials, manual QA

**Mock Strategy**:
- Use Moq or NSubstitute for interface mocking
- Create test fixtures with sample SSE response streams matching API documentation examples
- Mock HTTP responses for network error scenarios

---

## Resolved Technical Context

**SSE Parsing**: `System.Net.ServerSentEvents.SseParser` (NuGet package for .NET 8)  
**OpenAPI Spec**: Create from Microsoft Learn documentation, use Kiota to generate SDK  
**Testing**: Unit tests with mocks (CI/CD), integration tests against real API (manual/optional)

**All NEEDS CLARIFICATION items resolved. Ready for Phase 1 (Design).**
