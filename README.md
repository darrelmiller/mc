# M365 Copilot Chat CLI

Command-line tool for interacting with Microsoft 365 Copilot Chat API. Supports both one-shot queries and multi-turn interactive conversations.

## Features

- **Interactive Login**: Sign in with your Microsoft 365 account using Entra ID
- **Encrypted Token Cache**: Tokens are securely cached and refreshed automatically
- **One-Shot Mode**: Execute a single query and receive a response
- **Interactive REPL Mode**: Maintain multi-turn conversations with context (coming soon)
- **Streaming Responses**: Real-time Server-Sent Events streaming

## Prerequisites

- .NET 8.0 SDK or later
- Microsoft 365 Copilot license
- Microsoft 365 account with appropriate permissions

## Installation

### Build from Source

```powershell
# Clone repository
git clone https://github.com/microsoft/copilot-cli.git
cd copilot-cli

# Install Kiota CLI tool locally
dotnet tool restore

# Discover and install Kiota dependencies
dotnet kiota info -l csharp

# Generate SDK from OpenAPI spec
dotnet kiota generate

# Build project
dotnet build

# Run tool
dotnet run -- "Your query here"
```

### Install as Global Tool

```powershell
dotnet pack
dotnet tool install --global --add-source ./nupkg copilot-cli
```

## Configuration

### First-Time Setup

1. **Sign in with your Microsoft 365 account**:
   ```powershell
   dotnet run -- login
   ```

2. **A browser window will open** for you to authenticate with Microsoft Entra ID

3. **Tokens are cached securely**:
   - **Windows**: Encrypted using DPAPI (Data Protection API)
   - **macOS**: Stored in macOS Keychain
   - **Linux**: Stored in Linux Secret Service (GNOME Keyring, KWallet, etc.)

4. **Tokens refresh automatically** - you don't need to re-authenticate unless you explicitly log out

### Sign Out

To clear cached tokens:
```powershell
dotnet run -- logout
```

## Usage

### One-Shot Mode

Send a single query and exit:

```powershell
# First, authenticate
dotnet run -- login

# Then send queries
dotnet run -- "What are my upcoming meetings today?"
dotnet run -- "Summarize my recent emails from Sarah"
```

### Interactive Mode (Coming Soon)

Start an interactive conversation session:

```powershell
dotnet run
```

Type `exit` or `quit` to end the session.

## Exit Codes

- `0` - Success
- `1` - Authentication error (missing or invalid token)
- `2` - Permission denied (insufficient Graph API permissions)
- `3` - Network error
- `4` - Invalid input
- `5` - Conversation error

## Development

See [quickstart.md](specs/001-copilot-chat-cli/quickstart.md) for detailed usage instructions.

## License

MIT
