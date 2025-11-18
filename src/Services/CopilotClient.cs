using ApiSdk;
using ApiSdk.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization;
using System.Text;
using System.Text.Json;

namespace CopilotCli.Services;

/// <summary>
/// Implementation of ICopilotClient using Kiota-generated SDK.
/// </summary>
public class CopilotClient : ICopilotClient
{
    private readonly CopilotApi _client;
    private readonly IAuthProvider _authProvider;
    private readonly HttpClient _httpClient;

    public CopilotClient(IAuthProvider authProvider)
    {
        _authProvider = authProvider;
        _httpClient = new HttpClient();
        
        // Create authentication provider for Kiota
        var authenticationProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(authProvider));
        
        // Create HTTP client library request adapter
        var requestAdapter = new HttpClientRequestAdapter(authenticationProvider);
        
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
    }

    /// <summary>
    /// Sends a message to an existing conversation and receives a streaming response.
    /// </summary>
    public async Task<HttpResponseMessage> SendMessageAsync(string conversationId, string message, string? timeZone = null)
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

        // Construct the request manually to enable streaming
        var token = await _authProvider.GetTokenAsync();
        var requestUri = $"https://graph.microsoft.com/beta/copilot/conversations/{conversationId}/chatOverStream";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = await chatRequest.SerializeAsStringAsync("application/json");
        request.Content = new StreamContent(chatRequest.SerializeAsJsonStream());

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            
            if (statusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException($"Authentication failed: Token is invalid or expired. Response: {errorBody}");
            }
            else if (statusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    $"Forbidden: Insufficient permissions. Required permissions: " +
                    $"Sites.Read.All, Mail.Read, People.Read.All, OnlineMeetingTranscript.Read.All, " +
                    $"Chat.Read, ChannelMessage.Read.All, ExternalItem.Read.All. Response: {errorBody}");
            }
            else
            {
                throw new InvalidOperationException($"Request failed with status {statusCode}: {errorBody}");
            }
        }

        return response;
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
