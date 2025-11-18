# Implementation Plan: M365 Copilot Chat CLI

**Branch**: `001-copilot-chat-cli` | **Date**: 2025-11-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-copilot-chat-cli/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a command-line tool that interacts with M365 Copilot Chat API supporting two modes: one-shot queries (pass text as CLI argument) and interactive REPL (multi-turn conversations). Technical approach uses Kiota-generated SDK from OpenAPI spec for M365 Copilot API, .NET for cross-platform CLI development, and Server-Sent Events streaming for real-time responses. Authentication via Microsoft Entra ID (Azure AD) using MSAL with encrypted token caching.

## Technical Context

**Language/Version**: .NET 8.0 (LTS, cross-platform support for Windows/Linux/macOS)
**Primary Dependencies**: 
- Kiota CLI (local dotnet tool via manifest)
- Kiota-generated SDK for M365 Copilot API
- Microsoft.Identity.Client (MSAL) for authentication
- Microsoft.Identity.Client.Extensions.Msal for encrypted token caching
- System.CommandLine for CLI parsing
- System.Net.ServerSentEvents for SSE stream parsing

**Storage**: Encrypted token cache in user profile directory (DPAPI on Windows, Keychain on macOS, Secret Service on Linux)  
**Testing**: xUnit for unit tests, integration tests against M365 Copilot API with mocked dependencies  
**Target Platform**: Cross-platform CLI (Windows, Linux, macOS via .NET 8)
**Project Type**: single  
**Performance Goals**: <1s startup for one-shot mode, <2s for interactive mode, streaming latency <2s from API response  
**Constraints**: 
- Must use Kiota-generated SDK (constitutional requirement)
- Kiota installed as local tool via .NET tool manifest
- SDK generation manual step before build
- No manual HTTP code
- Interactive login via MSAL with Entra ID (Azure AD)
- Secure token caching with platform-native encryption

**Scale/Scope**: Single-user CLI tool, supports 50+ consecutive messages per session, handles streaming responses of arbitrary length, automatic token refresh

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle: Never write low-level HTTP calling code

**Requirement**: Always use Kiota generated SDKs for all HTTP interactions with Microsoft Graph and other services.

**Status**: ✅ PASS

**Compliance**: 
- M365 Copilot API interactions will use Kiota-generated SDK from OpenAPI specification
- No manual HttpClient or REST API calls will be written
- If OpenAPI spec doesn't exist for M365 Copilot Chat API, one will be inferred from documentation and used with Kiota

**Action Items for Phase 0 Research**:
- Locate existing OpenAPI spec for M365 Copilot Chat API
- If not found, create OpenAPI spec based on Microsoft Learn documentation
- Generate Kiota SDK from the spec
- Validate SDK supports required endpoints: POST /me/copilot/conversations and POST /me/copilot/conversations/{id}/chatOverStream

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Models/
│   ├── Conversation.cs       # Conversation entity
│   ├── Message.cs            # Message entity
│   └── ResponseEvent.cs      # SSE response event entity
├── Services/
│   ├── ICopilotClient.cs     # Interface for Copilot API operations
│   ├── CopilotClient.cs      # Implementation using Kiota SDK
│   ├── IAuthProvider.cs      # Interface for auth token provider
│   ├── AuthProvider.cs       # Environment variable token provider
│   └── StreamParser.cs       # SSE stream parsing logic
├── Cli/
│   ├── Program.cs            # Entry point, mode detection
│   ├── OneShotCommand.cs     # One-shot mode handler
│   ├── InteractiveCommand.cs # Interactive REPL handler
│   └── ErrorHandler.cs       # Error formatting and exit codes
└── Generated/
    └── CopilotSdk/           # Kiota-generated SDK code (gitignored, generated at build)

tests/
├── Unit/
│   ├── AuthProviderTests.cs
│   ├── StreamParserTests.cs
│   └── CommandTests.cs
└── Integration/
    └── CopilotClientTests.cs # Tests against real or mocked API

copilot-cli.csproj            # .NET project file
copilot-cli.sln               # Solution file
openapi.yaml                  # OpenAPI spec for M365 Copilot Chat API
kiota-config.json             # Kiota generation configuration
```

**Structure Decision**: Selected "Single project" structure as this is a standalone CLI tool with no web/mobile components. The `src/` directory contains core logic organized by responsibility (Models, Services, Cli), with Kiota-generated code isolated in `Generated/` to avoid mixing hand-written and generated code. Tests are organized by type (Unit vs Integration) following .NET conventions.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

*No constitutional violations identified. This section intentionally left empty.*

## Phase 1 Artifacts

**Status**: ✅ COMPLETE

Artifacts generated during Phase 1 (Design & Contracts):

1. **data-model.md**: Entity definitions for Conversation, Message, ResponseEvent, AuthToken with attributes, relationships, validation rules, and ERD
2. **contracts/openapi.yaml**: OpenAPI 3.0 specification defining M365 Copilot Chat API endpoints (POST /copilot/conversations, POST /copilot/conversations/{id}/chatOverStream) with complete schemas, security, and error responses
3. **quickstart.md**: User guide covering prerequisites, installation, configuration, usage examples (one-shot and interactive modes), exit codes, error messages, and troubleshooting

## Post-Design Constitution Check

*Re-evaluation after Phase 1 design completion.*

### Principle: Never write low-level HTTP calling code

**Status**: ✅ PASS

**Compliance Verification**: 
- OpenAPI specification created at `contracts/openapi.yaml` defines M365 Copilot API contract
- Kiota SDK will be generated from this spec (configured in `kiota-config.json`)
- Required dependencies discovered via `kiota info -l csharp` command
- `CopilotClient.cs` will consume Kiota-generated SDK classes, not HttpClient
- No manual REST calls, no JSON serialization code, no HTTP header manipulation
- SSE stream parsing uses `System.Net.ServerSentEvents.SseParser` (official Microsoft library), not manual HTTP streaming

**Conclusion**: Design maintains constitutional compliance. All HTTP interactions abstracted through Kiota SDK.

## Agent Context Update

**Status**: ✅ COMPLETE

GitHub Copilot instructions updated at `.github/agents/copilot-instructions.md`:
- Added .NET 8.0 technology stack
- Added Kiota CLI tools, System.CommandLine, SSE parsing library
- Noted stateless architecture (no database)
- Auto-generated project structure guidelines
