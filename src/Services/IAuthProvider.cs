namespace CopilotCli.Services;

/// <summary>
/// Provides authentication tokens for API requests.
/// </summary>
public interface IAuthProvider
{
    /// <summary>
    /// Retrieves the authentication token asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the bearer token.</returns>
    Task<string> GetTokenAsync();
}
