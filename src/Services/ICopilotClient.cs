using ApiSdk.Models;

namespace CopilotCli.Services;

/// <summary>
/// Client for interacting with the M365 Copilot API.
/// </summary>
public interface ICopilotClient
{
    /// <summary>
    /// Creates a new Copilot conversation.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created conversation.</returns>
    Task<CopilotConversation?> CreateConversationAsync();

    /// <summary>
    /// Sends a message to an existing conversation and receives a non-streaming response.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="message">The message text to send.</param>
    /// <param name="timeZone">Optional timezone identifier for locationHint.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated conversation.</returns>
    Task<CopilotConversation?> SendMessageNonStreamingAsync(string conversationId, string message, string? timeZone = null);

    /// <summary>
    /// Sends a message to an existing conversation and receives a streaming response.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="message">The message text to send.</param>
    /// <param name="timeZone">Optional timezone identifier for locationHint.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message with SSE stream.</returns>
    Task<HttpResponseMessage> SendMessageAsync(string conversationId, string message, string? timeZone = null);
}
