using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace CopilotCli.Services;

/// <summary>
/// Authentication provider that uses MSAL with encrypted token caching.
/// </summary>
public class AuthProvider : IAuthProvider
{
    private static IPublicClientApplication? _app;
    private static MsalCacheHelper? _cacheHelper;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets the authentication token using MSAL with cached tokens.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the bearer token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when authentication fails or no cached token is available.</exception>
    public async Task<string> GetTokenAsync()
    {
        var app = await GetOrCreateAppAsync();

        try
        {
            // Try to get accounts from cache
            var accounts = await app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            if (firstAccount != null)
            {
                try
                {
                    // Try silent authentication with cached token
                    var result = await app.AcquireTokenSilent(MsalConfig.Scopes, firstAccount)
                        .ExecuteAsync();
                    
                    return result.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    // Token expired or not in cache, need interactive login
                    throw new InvalidOperationException(
                        "Authentication required. Please run 'copilot-cli login' to sign in.");
                }
            }
            else
            {
                // No cached account, need interactive login
                throw new InvalidOperationException(
                    "Not authenticated. Please run 'copilot-cli login' to sign in.");
            }
        }
        catch (MsalException ex)
        {
            throw new InvalidOperationException($"Authentication failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Performs interactive login and caches the token.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LoginAsync()
    {
        var app = await GetOrCreateAppAsync();

        try
        {
            var result = await app.AcquireTokenInteractive(MsalConfig.Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();

            Console.WriteLine($"\nSuccessfully authenticated as: {result.Account.Username}");
            Console.WriteLine($"Token expires: {result.ExpiresOn.LocalDateTime}");
        }
        catch (MsalException ex)
        {
            throw new InvalidOperationException($"Login failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Logs out by clearing the token cache.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LogoutAsync()
    {
        var app = await GetOrCreateAppAsync();

        var accounts = await app.GetAccountsAsync();
        
        foreach (var account in accounts)
        {
            await app.RemoveAsync(account);
        }

        Console.WriteLine("Successfully logged out. Token cache cleared.");
    }

    /// <summary>
    /// Gets or creates the MSAL public client application with encrypted cache.
    /// </summary>
    private static async Task<IPublicClientApplication> GetOrCreateAppAsync()
    {
        if (_app != null)
        {
            return _app;
        }

        lock (_lock)
        {
            if (_app != null)
            {
                return _app;
            }

            // Build the public client application
            _app = PublicClientApplicationBuilder.Create(MsalConfig.ClientId)
                .WithAuthority(MsalConfig.Authority)
                .WithRedirectUri(MsalConfig.RedirectUri)
                .Build();
        }

        // Configure encrypted cache storage
        var cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            MsalConfig.CacheDirectory);

        Directory.CreateDirectory(cacheDirectory);

        var storageProperties = new StorageCreationPropertiesBuilder(
            MsalConfig.CacheFileName,
            cacheDirectory)
            .WithMacKeyChain(
                MsalConfig.KeyChainServiceName,
                MsalConfig.KeyChainAccountName)
            .Build();

        _cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
        _cacheHelper.RegisterCache(_app.UserTokenCache);

        return _app;
    }
}
