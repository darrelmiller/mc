namespace CopilotCli.Services;

/// <summary>
/// Configuration for MSAL authentication.
/// </summary>
public class MsalConfig
{
    /// <summary>
    /// Public client for the app
    /// </summary>
    public const string ClientId = "3c19e780-1d86-4317-800f-cc91904b4a25";

    /// <summary>
    /// The Azure AD tenant ID (common for multi-tenant).
    /// </summary>
    public const string TenantId = "common";

    /// <summary>
    /// The authority URL for authentication.
    /// </summary>
    public static string Authority => $"https://login.microsoftonline.com/{TenantId}";

    /// <summary>
    /// Required scopes for Microsoft Graph API access.
    /// </summary>
    public static readonly string[] Scopes = new[]
    {
        "https://graph.microsoft.com/Sites.Read.All",
        "https://graph.microsoft.com/Mail.Read",
        "https://graph.microsoft.com/People.Read.All",
        "https://graph.microsoft.com/OnlineMeetingTranscript.Read.All",
        "https://graph.microsoft.com/Chat.Read",
        "https://graph.microsoft.com/ChannelMessage.Read.All",
        "https://graph.microsoft.com/ExternalItem.Read.All"
    };

    /// <summary>
    /// The redirect URI for the public client application.
    /// </summary>
    public const string RedirectUri = "http://localhost";

    /// <summary>
    /// Cache file name for encrypted token storage.
    /// </summary>
    public const string CacheFileName = "msal_token_cache.dat";

    /// <summary>
    /// Cache directory name.
    /// </summary>
    public const string CacheDirectory = ".copilot-cli";

    /// <summary>
    /// Keychain service name for macOS.
    /// </summary>
    public const string KeyChainServiceName = "copilot-cli";

    /// <summary>
    /// Keychain account name for macOS.
    /// </summary>
    public const string KeyChainAccountName = "MSALCache";

    /// <summary>
    /// Linux keyring schema name.
    /// </summary>
    public const string LinuxKeyRingSchema = "com.microsoft.copilotcli.tokencache";

    /// <summary>
    /// Linux keyring collection.
    /// </summary>
    public const string LinuxKeyRingCollection = "default";

    /// <summary>
    /// Linux keyring label.
    /// </summary>
    public const string LinuxKeyRingLabel = "MSAL token cache for Copilot CLI";

    /// <summary>
    /// Linux keyring attribute key.
    /// </summary>
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttribute = 
        new KeyValuePair<string, string>("Version", "1");
}
