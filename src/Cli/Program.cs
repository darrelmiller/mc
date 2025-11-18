using CopilotCli.Cli;
using CopilotCli.Services;

namespace CopilotCli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var authProvider = new AuthProvider();

            // Check for commands
            if (args.Length > 0)
            {
                var command = args[0].ToLowerInvariant();

                switch (command)
                {
                    case "login":
                        var loginCommand = new LoginCommand(authProvider);
                        return await loginCommand.ExecuteAsync();

                    case "logout":
                        var logoutCommand = new LogoutCommand(authProvider);
                        return await logoutCommand.ExecuteAsync();

                    case "help":
                    case "--help":
                    case "-h":
                        DisplayHelp();
                        return ErrorHandler.Success;

                    default:
                        // Check for --stream or -s flag
                        bool useStreaming = false;
                        var queryArgs = new List<string>();
                        
                        foreach (var arg in args)
                        {
                            if (arg == "--stream" || arg == "-s")
                            {
                                useStreaming = true;
                            }
                            else
                            {
                                queryArgs.Add(arg);
                            }
                        }
                        
                        var query = string.Join(" ", queryArgs);
                        var copilotClient = new CopilotClient(authProvider);
                        var oneShotCommand = new OneShotCommand(authProvider, copilotClient);
                        return await oneShotCommand.ExecuteAsync(query, useStreaming);
                }
            }
            else
            {
                // Interactive mode (not yet implemented)
                Console.WriteLine("M365 Copilot Chat CLI");
                Console.WriteLine();
                Console.WriteLine("Interactive mode not yet implemented.");
                Console.WriteLine("Run 'copilot-cli help' for usage information.");
                Console.WriteLine();
                return ErrorHandler.InvalidInput;
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.WriteError($"Unexpected error: {ex.Message}", ErrorHandler.ConversationError);
            return ErrorHandler.ConversationError;
        }
    }

    static void DisplayHelp()
    {
        Console.WriteLine("M365 Copilot Chat CLI - Interactive chat with Microsoft 365 Copilot");
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine("  copilot-cli login                  Sign in with Microsoft Entra");
        Console.WriteLine("  copilot-cli logout                 Sign out and clear cached tokens");
        Console.WriteLine("  copilot-cli [--stream|-s] \"<query>\" Send a one-shot query");
        Console.WriteLine("  copilot-cli                        Start interactive mode (coming soon)");
        Console.WriteLine("  copilot-cli help                   Display this help message");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  --stream, -s                       Use streaming endpoint for response");
        Console.WriteLine();
        Console.WriteLine("AUTHENTICATION:");
        Console.WriteLine("  Before using the CLI, you must authenticate:");
        Console.WriteLine("    1. Run 'copilot-cli login'");
        Console.WriteLine("    2. Sign in with your Microsoft 365 account in the browser");
        Console.WriteLine("    3. Tokens are cached securely and refreshed automatically");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  copilot-cli login");
        Console.WriteLine("  copilot-cli \"What meetings do I have today?\"");
        Console.WriteLine("  copilot-cli --stream \"Summarize my recent emails from John\"");
        Console.WriteLine("  copilot-cli -s \"What's on my calendar?\"");
        Console.WriteLine("  copilot-cli logout");
        Console.WriteLine();
        Console.WriteLine("EXIT CODES:");
        Console.WriteLine("  0 - Success");
        Console.WriteLine("  1 - Authentication error");
        Console.WriteLine("  2 - Permission denied");
        Console.WriteLine("  3 - Network error");
        Console.WriteLine("  4 - Invalid input");
        Console.WriteLine("  5 - Conversation error");
    }
}
