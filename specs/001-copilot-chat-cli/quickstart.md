# M365 Copilot Chat CLI - Quick Start Guide

## Overview

The M365 Copilot Chat CLI is a command-line tool for interacting with Microsoft 365 Copilot. It supports both one-shot queries and multi-turn interactive conversations.

## Prerequisites

1. **Microsoft 365 Copilot License**: Active Copilot for Microsoft 365 license
2. **Bearer Token**: Valid Microsoft Graph access token with required permissions
3. **.NET Runtime**: .NET 8.0 or later installed

### Required Permissions

Your bearer token must have all of the following Microsoft Graph delegated permissions:
- `Sites.Read.All`
- `Mail.Read`
- `People.Read.All`
- `OnlineMeetingTranscript.Read.All`
- `Chat.Read`
- `ChannelMessage.Read.All`
- `ExternalItem.Read.All`

## Installation

Install the tool as a .NET global tool:

```powershell
dotnet tool install --global M365.Copilot.Cli
```

Or install locally in a project:

```powershell
dotnet tool install M365.Copilot.Cli
```

## Configuration

Set your Microsoft Graph bearer token as an environment variable:

### PowerShell (Windows)
```powershell
$env:M365_COPILOT_TOKEN = "your-bearer-token-here"
```

### Bash/Zsh (macOS/Linux)
```bash
export M365_COPILOT_TOKEN="your-bearer-token-here"
```

### Persistent Configuration (PowerShell)
Add to your PowerShell profile (`$PROFILE`):
```powershell
$env:M365_COPILOT_TOKEN = "your-bearer-token-here"
```

## Usage

### One-Shot Mode

Send a single query and exit:

```powershell
copilot-cli "What are my upcoming meetings today?"
```

```powershell
copilot-cli "Summarize emails from last week about the Q4 project"
```

**Output**: The tool will print Copilot's response to stdout and exit with code 0.

### Interactive Mode (REPL)

Start an interactive conversation session:

```powershell
copilot-cli
```

**Example Session**:
```
M365 Copilot Chat CLI v1.0.0
Type 'exit' or 'quit' to end the session.

> What are my top priority tasks?

Based on your emails and calendar, here are your top priorities:
1. Q4 Planning Review - Due tomorrow at 2 PM
2. Team Sync Meeting - Today at 3 PM
3. Budget Approval - Pending your review

> Who sent me the Q4 planning document?

The Q4 planning document was sent by Jane Smith on March 15th.

> exit

Goodbye!
```

**Commands**:
- Type your message and press Enter to send
- Type `exit` or `quit` to end the session
- Press Ctrl+C to interrupt

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Authentication error (missing or invalid token) |
| 2 | Permission denied (insufficient Graph permissions) |
| 3 | Network error (unable to reach Microsoft Graph API) |
| 4 | Invalid input (malformed query or parameters) |
| 5 | Conversation error (conversation not found or expired) |

## Common Error Messages

### "Authentication failed: M365_COPILOT_TOKEN environment variable not set"
**Solution**: Set the environment variable with your bearer token (see Configuration section)

### "Forbidden: Insufficient permissions"
**Solution**: Ensure your token has all required Microsoft Graph permissions listed in Prerequisites

### "Conversation not found (404)"
**Solution**: The conversation may have expired or been deleted. In interactive mode, the tool will automatically create a new conversation.

### "Network error: Unable to reach Microsoft Graph"
**Solution**: Check your internet connection and verify graph.microsoft.com is accessible

## Troubleshooting

### Token Expiration
Bearer tokens typically expire after 60-90 minutes. If you receive authentication errors:
1. Obtain a new bearer token
2. Update the `M365_COPILOT_TOKEN` environment variable
3. Restart the CLI tool

### Slow Response Times
Copilot responses are streamed in real-time. Delays may occur if:
- Your query requires searching large amounts of data
- Network latency is high
- Microsoft 365 services are experiencing high load

### Empty Responses
If Copilot returns empty or minimal responses:
- Ensure your query is specific and actionable
- Verify Copilot has access to relevant data sources (emails, files, calendar)
- Check that your M365 data sources are properly indexed

## Advanced Usage

### Custom Timezone
By default, the tool uses your system timezone. The timezone is used for time-aware queries like "meetings today" or "emails this week".

### Debugging
For verbose output including API calls and raw SSE events, run with debug flag:
```powershell
copilot-cli --debug "your query"
```

## Support

For issues or questions:
- API Documentation: [Microsoft 365 Copilot Chat API](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/api/ai-services/chat/overview)
- Microsoft Graph Support: [Microsoft Graph Support](https://developer.microsoft.com/en-us/graph/support)

## Version Information

Current version: 1.0.0
API Version: Microsoft Graph Beta
Minimum .NET Version: 8.0
