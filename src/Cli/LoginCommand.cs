using CopilotCli.Services;

namespace CopilotCli.Cli;

/// <summary>
/// Handles the login command for interactive authentication.
/// </summary>
public class LoginCommand
{
    private readonly AuthProvider _authProvider;

    public LoginCommand(AuthProvider authProvider)
    {
        _authProvider = authProvider;
    }

    /// <summary>
    /// Executes the login flow using MSAL interactive authentication.
    /// </summary>
    /// <returns>Exit code (0 for success, non-zero for errors).</returns>
    public async Task<int> ExecuteAsync()
    {
        try
        {
            Console.WriteLine("Opening browser for Microsoft Entra authentication...");
            Console.WriteLine("Please sign in with your Microsoft 365 account.");
            Console.WriteLine();

            await _authProvider.LoginAsync();

            Console.WriteLine("\nLogin successful! You can now use 'copilot-cli' to chat with M365 Copilot.");
            
            return ErrorHandler.Success;
        }
        catch (InvalidOperationException ex)
        {
            ErrorHandler.WriteError($"Login failed: {ex.Message}", ErrorHandler.AuthError);
            return ErrorHandler.AuthError;
        }
        catch (Exception ex)
        {
            ErrorHandler.WriteError($"Unexpected error during login: {ex.Message}", ErrorHandler.AuthError);
            return ErrorHandler.AuthError;
        }
    }
}
