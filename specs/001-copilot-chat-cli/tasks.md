# Tasks: M365 Copilot Chat CLI

**Input**: Design documents from `specs/001-copilot-chat-cli/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml

**Tests**: Not explicitly requested in specification - tasks focus on implementation only.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create project structure with src/, tests/, specs/ directories at repository root
- [X] T002 Initialize .NET 8.0 project with copilot-cli.csproj including project metadata (name, version, target framework)
- [X] T003 [P] Add solution file copilot-cli.sln to organize project
- [X] T004 [P] Copy contracts/openapi.yaml from specs/001-copilot-chat-cli/contracts/ to repository root
- [X] T005 [P] Create .gitignore with .NET patterns (bin/, obj/, Generated/, *.user, *.suo)
- [X] T006 [P] Create README.md with project overview and quickstart instructions based on specs/001-copilot-chat-cli/quickstart.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 Create .NET tool manifest with `dotnet new tool-manifest` and install Kiota with `dotnet tool install --local Microsoft.OpenApi.Kiota`
- [X] T008 [P] Add NuGet package System.CommandLine to copilot-cli.csproj for CLI argument parsing
- [X] T009 [P] Add NuGet package System.Net.ServerSentEvents to copilot-cli.csproj for SSE stream parsing
- [X] T010 [P] Add NuGet package xUnit to copilot-cli.csproj for testing framework (tests/Unit and tests/Integration)
- [X] T011 Create kiota-config.json at repository root with configuration to generate SDK from openapi.yaml into src/Generated/CopilotSdk/
- [X] T012 Run `kiota info -l csharp` to discover required Kiota dependencies, then add them to copilot-cli.csproj (e.g., Microsoft.Kiota.Http.HttpClientLibrary, Microsoft.Kiota.Serialization.Json)
- [X] T013 Run `dotnet kiota generate` to create SDK from openapi.yaml into src/Generated/CopilotSdk/ directory including all model classes (CopilotConversation, CopilotMessage, ChatRequest, etc.) - manual step, document in README
- [X] T014 [P] Create src/Services/IAuthProvider.cs interface with GetTokenAsync() method signature
- [X] T015 [P] Create src/Services/ICopilotClient.cs interface with CreateConversationAsync() and SendMessageAsync() method signatures
- [X] T016 [P] Create src/Cli/ErrorHandler.cs with static methods for error formatting and exit code constants (0=success, 1=auth error, 2=permission denied, 3=network error, 4=invalid input, 5=conversation error)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - One-Shot Query Mode (Priority: P1) 🎯 MVP

**Goal**: Users can execute a one-shot query by passing text as a command-line argument and receive a streamed response from M365 Copilot

**Independent Test**: Run `copilot-cli "What is the weather today?"` with valid M365_COPILOT_TOKEN environment variable and verify response streams to stdout with exit code 0

### Implementation for User Story 1

- [X] T017 [US1] Implement src/Services/AuthProvider.cs reading M365_COPILOT_TOKEN environment variable and exposing it via GetTokenAsync() method
- [X] T018 [US1] Implement src/Services/StreamParser.cs with ParseSseStreamAsync() method using System.Net.ServerSentEvents.SseParser to parse SSE events from HttpResponseMessage stream
- [X] T019 [US1] Implement src/Services/CopilotClient.cs with CreateConversationAsync() method calling Kiota-generated SDK's Copilot.Conversations.Post() endpoint, returning CopilotConversation model
- [X] T020 [US1] Implement SendMessageAsync() method in src/Services/CopilotClient.cs calling Kiota-generated SDK's Copilot.Conversations[id].ChatOverStream.Post() with HttpCompletionOption.ResponseHeadersRead
- [X] T021 [US1] Add ExtractMessageText() method to src/Services/StreamParser.cs to parse Kiota-generated CopilotConversation objects from SSE data and extract messages[].text field
- [X] T022 [US1] Create src/Cli/Program.cs with Main() entry point detecting command-line arguments (args.Length > 0 = one-shot, args.Length == 0 = interactive)
- [X] T023 [US1] Implement src/Cli/OneShotCommand.cs with ExecuteAsync() method accepting string query parameter
- [X] T024 [US1] Wire up OneShotCommand in Program.cs: read token via AuthProvider, create CopilotClient, call CreateConversationAsync(), call SendMessageAsync(), parse stream via StreamParser, write response to stdout
- [X] T025 [US1] Add error handling in OneShotCommand.cs for missing token (exit code 1), authentication failures (401/403, exit code 2), network errors (exit code 3), and general exceptions with stderr output
- [X] T026 [US1] Add timezone detection in OneShotCommand.cs using TimeZoneInfo.Local.Id for locationHint.timeZone parameter in Kiota-generated ChatRequest model
- [ ] T027 [US1] Test one-shot mode manually: set M365_COPILOT_TOKEN, run `copilot-cli "test query"`, verify streaming output and exit code 0

**Checkpoint**: At this point, User Story 1 should be fully functional - one-shot queries work end-to-end

---

## Phase 4: User Story 2 - Interactive REPL Mode (Priority: P2)

**Goal**: Users can launch interactive mode without arguments, send multiple messages maintaining conversation context, and exit gracefully

**Independent Test**: Run `copilot-cli` without arguments, send 3+ messages referencing previous context, type `exit` to quit, verify all responses maintain context

### Implementation for User Story 2

- [ ] T028 [P] [US2] Create src/Cli/InteractiveCommand.cs with ExecuteAsync() method for REPL loop
- [ ] T029 [US2] Implement prompt display in InteractiveCommand.cs showing "M365 Copilot Chat CLI v1.0.0" welcome message and "copilot> " input prompt
- [ ] T030 [US2] Implement conversation initialization in InteractiveCommand.cs: create conversation once at session start, store conversation ID for entire session
- [ ] T031 [US2] Implement message input loop in InteractiveCommand.cs: read user input from Console.ReadLine(), check for exit commands ("exit", "quit", case-insensitive)
- [ ] T032 [US2] Implement message sending in InteractiveCommand.cs: call CopilotClient.SendMessageAsync() with stored conversation ID, same StreamParser logic as one-shot mode
- [ ] T033 [US2] Add response display in InteractiveCommand.cs: stream response to stdout, return to prompt after each response completes
- [ ] T034 [US2] Add Ctrl+C handling in InteractiveCommand.cs: register Console.CancelKeyPress handler to gracefully exit interactive session
- [ ] T035 [US2] Add error recovery in InteractiveCommand.cs: catch exceptions during message send, display error to stderr, return to prompt (don't crash session)
- [ ] T036 [US2] Wire up InteractiveCommand in Program.cs: when args.Length == 0, instantiate InteractiveCommand and call ExecuteAsync()
- [ ] T037 [US2] Add empty message handling in InteractiveCommand.cs: if user presses Enter without typing, display "Message cannot be empty" and re-prompt
- [ ] T038 [US2] Test interactive mode manually: launch `copilot-cli`, send message "What is my name?", send follow-up "What did I just ask you?", verify context maintained, type `exit`, verify graceful shutdown

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - one-shot and interactive modes fully functional

---

## Phase 5: User Story 3 - Token-Based Authentication (Priority: P3)

**Goal**: Users receive clear error messages when M365_COPILOT_TOKEN is missing or invalid, with guidance on how to obtain a token

**Independent Test**: Run tool without M365_COPILOT_TOKEN set and verify helpful error message; run with expired token and verify 401/403 error is clearly reported

### Implementation for User Story 3

- [ ] T039 [US3] Add token validation in src/Services/AuthProvider.cs GetTokenAsync() method: throw descriptive exception if M365_COPILOT_TOKEN environment variable is null or empty
- [ ] T040 [US3] Update error handling in src/Cli/ErrorHandler.cs to detect AuthProvider exceptions and format user-friendly message: "Authentication failed: M365_COPILOT_TOKEN environment variable not set. Obtain a token from Azure Portal and set the environment variable."
- [ ] T041 [US3] Add HTTP 401 detection in src/Services/CopilotClient.cs: catch HttpRequestException with 401 status, throw custom exception with message "Authentication failed: Token is invalid or expired"
- [ ] T042 [US3] Add HTTP 403 detection in src/Services/CopilotClient.cs: catch HttpRequestException with 403 status, throw custom exception with message "Forbidden: Insufficient permissions. Required permissions: Sites.Read.All, Mail.Read, People.Read.All, OnlineMeetingTranscript.Read.All, Chat.Read, ChannelMessage.Read.All, ExternalItem.Read.All"
- [ ] T043 [US3] Update OneShotCommand.cs to catch authentication exceptions and call ErrorHandler with exit code 1 (auth error) or 2 (permission denied)
- [ ] T044 [US3] Update InteractiveCommand.cs to catch authentication exceptions during session initialization and display error before exiting (don't enter REPL loop if auth fails)
- [ ] T045 [US3] Test authentication error handling: unset M365_COPILOT_TOKEN, run tool in one-shot mode, verify error message mentions environment variable and exit code 1
- [ ] T046 [US3] Test permission error handling: use token without required permissions (if available for testing), verify error message lists required permissions and exit code 2

**Checkpoint**: All user stories should now be independently functional - one-shot, interactive, and authentication with helpful errors

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final validation

- [ ] T047 [P] Add XML documentation comments to all public methods in src/Services/ and src/Cli/ for IntelliSense support
- [ ] T048 [P] Add logging framework (Microsoft.Extensions.Logging) for debug mode (optional --debug flag)
- [ ] T049 [P] Implement --debug flag in Program.cs to enable verbose output showing API calls and raw SSE events
- [ ] T050 [P] Add --version flag in Program.cs to display tool version from assembly metadata
- [ ] T051 [P] Add --help flag in Program.cs using System.CommandLine to auto-generate usage documentation
- [ ] T052 Update README.md at repository root with complete installation, configuration, and usage instructions from specs/001-copilot-chat-cli/quickstart.md
- [ ] T053 Add CHANGELOG.md at repository root documenting v1.0.0 release with features: one-shot mode, interactive mode, token-based auth
- [ ] T054 Configure copilot-cli.csproj for packaging as .NET global tool: PackAsTool=true, ToolCommandName=copilot-cli, PackageId, Authors, Description
- [ ] T055 Test .NET global tool installation: run `dotnet pack`, `dotnet tool install --global --add-source ./nupkg copilot-cli`, verify `copilot-cli --version` works
- [ ] T056 Run through all quickstart.md scenarios from specs/001-copilot-chat-cli/quickstart.md to validate end-to-end functionality
- [ ] T057 Code review: verify no manual HttpClient usage (constitutional requirement), all API calls use Kiota-generated SDK, SSE parsing uses System.Net.ServerSentEvents, all models are Kiota-generated
- [ ] T058 Performance validation: verify one-shot startup <1s, interactive startup <2s, streaming latency <2s per success criteria

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational (Phase 2) completion
  - User Story 1 (Phase 3): Can start after Foundational - No dependencies on other stories
  - User Story 2 (Phase 4): Can start after Foundational - Reuses US1 components (AuthProvider, CopilotClient, StreamParser) but independently testable
  - User Story 3 (Phase 5): Can start after Foundational - Enhances US1 and US2 error handling but independently testable
- **Polish (Phase 6)**: Depends on all desired user stories (Phase 3-5) being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundational → US1 implementation → MVP ready ✅
- **User Story 2 (P2)**: Foundational → US1 components (reuses AuthProvider, CopilotClient, StreamParser) → US2 interactive logic
- **User Story 3 (P3)**: Foundational → US1 & US2 error handlers → Enhanced auth error messaging

### Within Each User Story

**User Story 1 Flow**:
1. AuthProvider (T017) - blocks all API calls
2. StreamParser (T018) - independent
3. CopilotClient.CreateConversationAsync (T019) - depends on AuthProvider and Kiota-generated models
4. CopilotClient.SendMessageAsync (T020) - depends on AuthProvider and Kiota-generated models
5. StreamParser.ExtractMessageText (T021) - depends on T018 and Kiota-generated models
6. Program.cs (T022) - depends on all prior
7. OneShotCommand (T023) - can be parallel with T022
8. Wire up OneShotCommand (T024) - depends on all components
9. Error handling (T025-T026) - depends on T024
10. Manual test (T027) - validates all above

**User Story 2 Flow**:
1. InteractiveCommand scaffolding (T028-T029) in parallel
2. Conversation initialization (T030) - depends on CopilotClient from US1
3. Input loop (T031) - depends on T029
4. Message sending (T032) - depends on CopilotClient and StreamParser from US1
5. Response display (T033) - depends on T032
6. Ctrl+C handling (T034) - can be parallel
7. Error recovery (T035) - depends on T033
8. Wire up in Program.cs (T036) - depends on all prior
9. Empty message handling (T037) - depends on T031
10. Manual test (T038) - validates all above

**User Story 3 Flow**:
1. AuthProvider validation (T039) - enhances US1 T017
2. ErrorHandler updates (T040) - enhances Foundational T016
3. HTTP 401 detection (T041) - enhances US1 T019-T020
4. HTTP 403 detection (T042) - enhances US1 T019-T020
5. OneShotCommand error updates (T043) - depends on T040-T042
6. InteractiveCommand error updates (T044) - depends on T040-T042
7. Test auth errors (T045-T046) - validates all above

### Parallel Opportunities

**Phase 1 (Setup)**: T003, T004, T005, T006 can run in parallel (all marked [P])

**Phase 2 (Foundational)**: 
- T008, T009, T010 (NuGet packages) in parallel [P]
- T014, T015, T016 (interfaces and error handler) in parallel [P]
- T013 (Kiota generation including all models) depends on T007, T011, T012

**Phase 3 (User Story 1)**:
- T017 (AuthProvider) and T018 (StreamParser) can start in parallel after Kiota generation (T013)
- After AuthProvider (T017) completes: T019 (CreateConversation) can proceed
- T018 and T019 can run in parallel

**Phase 4 (User Story 2)**:
- T028, T029 (InteractiveCommand scaffolding) in parallel

**Phase 5 (User Story 3)**:
- No parallel opportunities (all tasks enhance existing components sequentially)

**Phase 6 (Polish)**:
- T047, T048, T049, T050, T051 (documentation and flags) in parallel [P]

**Cross-Story Parallelization** (if multiple developers):
- After Foundational completes: Developer A → US1, Developer B → US2 (can work in parallel until US2 needs to integrate US1 components)
- After US1 completes: Developer A → US3 error handling for US1, Developer B → US2 continuation

---

## Parallel Example: User Story 1

```bash
# After Kiota SDK generation (T013), launch these in parallel:
Task T017: "Implement src/Services/AuthProvider.cs reading M365_COPILOT_TOKEN..."
Task T018: "Implement src/Services/StreamParser.cs with ParseSseStreamAsync()..."

# After AuthProvider (T017) is done, T019 can proceed:
Task T019: "Implement src/Services/CopilotClient.cs with CreateConversationAsync()..."
# Note: T018 and T019 can run in parallel
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T006)
2. Complete Phase 2: Foundational (T007-T016) - CRITICAL (includes Kiota SDK generation with all models)
3. Complete Phase 3: User Story 1 (T017-T027)
4. **STOP and VALIDATE**: Run `copilot-cli "test query"` and verify streaming response
5. Ready for demo/deployment with basic one-shot functionality ✅

### Incremental Delivery

1. **Foundation** (Phase 1-2) → Kiota SDK generated, project structure ready
2. **MVP** (Phase 3) → One-shot mode working → Deploy/Demo ✅
3. **Enhanced UX** (Phase 4) → Interactive REPL mode → Deploy/Demo ✅
4. **Production Ready** (Phase 5) → Better auth errors → Deploy/Demo ✅
5. **Polished** (Phase 6) → Documentation, packaging, --help/--version → Final release ✅

Each increment adds value without breaking previous functionality.

### Parallel Team Strategy

With 2 developers:

1. **Together**: Complete Setup (Phase 1) + Foundational (Phase 2)
2. **Split**:
   - Developer A: User Story 1 (Phase 3) → User Story 3 auth errors for US1 (Phase 5)
   - Developer B: User Story 2 (Phase 4) → User Story 3 auth errors for US2 (Phase 5)
3. **Together**: Polish (Phase 6) and final validation

---

## Notes

- **Constitutional Compliance**: No manual HttpClient usage - all API calls via Kiota-generated SDK (verified in T057)
- **Model Generation**: All models (CopilotConversation, CopilotMessage, ChatRequest, etc.) are generated by Kiota from OpenAPI spec - no manual model classes
- **Testing Strategy**: Manual testing approach (no automated tests requested in spec) with clear validation checkpoints
- **File Path Convention**: Single project structure - all source in `src/`, all tests in `tests/`, Kiota-generated code in `src/Generated/CopilotSdk/`
- **[P] markers**: Tasks that can run in parallel because they touch different files with no interdependencies
- **[Story] labels**: Map tasks to user stories from spec.md (US1=P1 one-shot, US2=P2 interactive, US3=P3 auth)
- **Checkpoints**: Stop after each user story phase to validate independently before proceeding
- **Exit Codes**: 0=success, 1=auth error, 2=permission denied, 3=network error, 4=invalid input, 5=conversation error
- **Commit Strategy**: Commit after each task or logical group of parallel tasks for clean history
