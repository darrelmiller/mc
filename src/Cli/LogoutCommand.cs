using CopilotCli.Services;

namespace CopilotCli.Cli;

/// <summary>
/// Handles the logout command to clear cached tokens.
/// </summary>
public class LogoutCommand
{
    private readonly AuthProvider _authProvider;

    public LogoutCommand(AuthProvider authProvider)
    {
        _authProvider = authProvider;
    }

    /// <summary>
    /// Executes the logout flow by clearing the token cache.
    /// </summary>
    /// <returns>Exit code (0 for success, non-zero for errors).</returns>
    public async Task<int> ExecuteAsync()
    {
        try
        {
            await _authProvider.LogoutAsync();
            return ErrorHandler.Success;
        }
        catch (Exception ex)
        {
            ErrorHandler.WriteError($"Logout failed: {ex.Message}", ErrorHandler.AuthError);
            return ErrorHandler.AuthError;
        }
    }
}
