namespace CopilotCli.Cli;

/// <summary>
/// Handles error formatting and exit codes for the CLI application.
/// </summary>
public static class ErrorHandler
{
    // Exit codes
    public const int Success = 0;
    public const int AuthError = 1;
    public const int PermissionDenied = 2;
    public const int NetworkError = 3;
    public const int InvalidInput = 4;
    public const int ConversationError = 5;

    /// <summary>
    /// Formats an error message for display to stderr.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>Formatted error string.</returns>
    public static string FormatError(string message)
    {
        return $"Error: {message}";
    }

    /// <summary>
    /// Writes an error message to stderr and returns the appropriate exit code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exitCode">The exit code to return.</param>
    public static void WriteError(string message, int exitCode)
    {
        Console.Error.WriteLine(FormatError(message));
        Environment.ExitCode = exitCode;
    }
}
