using ApiSdk;
using ApiSdk.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization;
using Microsoft.Kiota.Serialization.Json;
using System.Text;
using System.Text.Json;

namespace CopilotCli.Services;

/// <summary>
/// Implementation of ICopilotClient using Kiota-generated SDK.
/// </summary>
public class CopilotClient : ICopilotClient
{
    private readonly CopilotApi _client;

    public CopilotClient(IAuthProvider authProvider)
    {

        // Create authentication provider for Kiota
        var authenticationProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(authProvider));

        // Create HTTP client library request adapter
        var requestAdapter = new HttpClientRequestAdapter(authenticationProvider);

        // Register JSON serialization factory
        ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
        ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();

        // Initialize Kiota client
        _client = new CopilotApi(requestAdapter);
    }

    /// <summary>
    /// Creates a new Copilot conversation.
    /// </summary>
    public async Task<CopilotConversation?> CreateConversationAsync()
    {
        try
        {
            var conversation = await _client.Copilot.Conversations.PostAsync(new ApiSdk.Copilot.Conversations.ConversationsPostRequestBody());
            return conversation;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("Authentication failed: Token is invalid or expired", ex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "Forbidden: Insufficient permissions. Required permissions: " +
                "Sites.Read.All, Mail.Read, People.Read.All, OnlineMeetingTranscript.Read.All, " +
                "Chat.Read, ChannelMessage.Read.All, ExternalItem.Read.All", ex);
        }
        catch (ApiSdk.Models.CopilotConversation500Error ex)
        {
            throw new InvalidOperationException($"Server error: {ex.Error?.Message ?? "An internal server error occurred"}", ex);
        }
    }

    /// <summary>
    /// Sends a message to an existing conversation and receives a non-streaming response.
    /// </summary>
    public async Task<CopilotConversation?> SendMessageAsync(string conversationId, string message, string? timeZone = null)
    {
        var chatRequest = new ChatRequest
        {
            Message = new MessageParameter
            {
                Text = message
            },
            LocationHint = new LocationHint
            {
                TimeZone = timeZone ?? ConvertToIanaTimeZone(TimeZoneInfo.Local.Id)
            }
        };

        try
        {
            var conversationGuid = Guid.Parse(conversationId);
            var response = await _client.Copilot.Conversations[conversationGuid].Chat.PostAsync(chatRequest);
            return response;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("Authentication failed: Token is invalid or expired", ex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "Forbidden: Insufficient permissions. Required permissions: " +
                "Sites.Read.All, Mail.Read, People.Read.All, OnlineMeetingTranscript.Read.All, " +
                "Chat.Read, ChannelMessage.Read.All, ExternalItem.Read.All", ex);
        }
        catch (ApiSdk.Models.CopilotConversation500Error ex)
        {
            throw new InvalidOperationException($"Server error: {ex.Error?.Message ?? "An internal server error occurred"}", ex);
        }
    }

    /// <summary>
    /// Sends a message to an existing conversation and receives a streaming response.
    /// </summary>
    public async Task<Stream?> SendStreamingMessageAsync(string conversationId, string message, string? timeZone = null)
    {
        var chatRequest = new ChatRequest
        {
            Message = new MessageParameter
            {
                Text = message
            },
            LocationHint = new LocationHint
            {
                TimeZone = timeZone ?? ConvertToIanaTimeZone(TimeZoneInfo.Local.Id)
            }
        };

        try
        {
            var conversationGuid = Guid.Parse(conversationId);
            var stream = await _client.Copilot.Conversations[conversationGuid].ChatOverStream.PostAsync(chatRequest);
            return stream;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("Authentication failed: Token is invalid or expired", ex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "Forbidden: Insufficient permissions. Required permissions: " +
                "Sites.Read.All, Mail.Read, People.Read.All, OnlineMeetingTranscript.Read.All, " +
                "Chat.Read, ChannelMessage.Read.All, ExternalItem.Read.All", ex);
        }
        catch (ApiSdk.Models.ChatOverStream500Error ex)
        {
            throw new InvalidOperationException($"Server error: {ex.Error?.Message ?? "An internal server error occurred"}", ex);
        }
    }

    /// <summary>
    /// Converts Windows timezone ID to IANA timezone format.
    /// </summary>
    private static string ConvertToIanaTimeZone(string windowsTimeZoneId)
    {
        // Try to get IANA ID directly from TimeZoneInfo (available in .NET 6+)
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
        
        // TimeZoneInfo.HasIanaId is available in .NET 6+
        if (timeZone.HasIanaId)
        {
            return timeZone.Id;
        }

        // Fallback: Common Windows to IANA mappings
        return windowsTimeZoneId switch
        {
            "Pacific Standard Time" => "America/Los_Angeles",
            "Mountain Standard Time" => "America/Denver",
            "Central Standard Time" => "America/Chicago",
            "Eastern Standard Time" => "America/New_York",
            "GMT Standard Time" => "Europe/London",
            "UTC" => "UTC",
            "W. Europe Standard Time" => "Europe/Paris",
            "Central Europe Standard Time" => "Europe/Warsaw",
            "Romance Standard Time" => "Europe/Paris",
            "Central European Standard Time" => "Europe/Belgrade",
            "AUS Eastern Standard Time" => "Australia/Sydney",
            "E. Australia Standard Time" => "Australia/Brisbane",
            "AUS Central Standard Time" => "Australia/Darwin",
            "China Standard Time" => "Asia/Shanghai",
            "India Standard Time" => "Asia/Kolkata",
            "Tokyo Standard Time" => "Asia/Tokyo",
            _ => timeZone.Id // Return as-is if no mapping found
        };
    }

    /// <summary>
    /// Token provider implementation for Kiota authentication.
    /// </summary>
    private class TokenProvider : IAccessTokenProvider
    {
        private readonly IAuthProvider _authProvider;

        public TokenProvider(IAuthProvider authProvider)
        {
            _authProvider = authProvider;
        }

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            return _authProvider.GetTokenAsync();
        }

        public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
    }
}
