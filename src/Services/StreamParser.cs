using System.Net.ServerSentEvents;
using System.Text.Json;
using ApiSdk.Models;

namespace CopilotCli.Services;

/// <summary>
/// Parses Server-Sent Events streams from the Copilot API.
/// </summary>
public class StreamParser
{
    /// <summary>
    /// Parses SSE events from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the SSE data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of SSE events.</returns>
    public static async IAsyncEnumerable<SseItem<string>> ParseSseStreamAsync(
        Stream stream,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var sseEvent in SseParser.Create(stream).EnumerateAsync(cancellationToken))
        {
            yield return sseEvent;
        }
    }

    /// <summary>
    /// Parses SSE events from an HTTP response stream.
    /// </summary>
    /// <param name="response">The HTTP response message containing the SSE stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of SSE events.</returns>
    [Obsolete("Use the overload that accepts Stream directly")]
    public static async IAsyncEnumerable<SseItem<string>> ParseSseStreamAsync(
        HttpResponseMessage response,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        await foreach (var sseEvent in SseParser.Create(stream).EnumerateAsync(cancellationToken))
        {
            yield return sseEvent;
        }
    }

    /// <summary>
    /// Extracts message text from a CopilotConversation object parsed from SSE data.
    /// </summary>
    /// <param name="jsonData">JSON string containing the CopilotConversation object.</param>
    /// <returns>The extracted message text, or null if not found.</returns>
    public static string? ExtractMessageText(string jsonData)
    {
        try
        {
            var conversation = JsonSerializer.Deserialize<CopilotConversation>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Extract the latest message text from the messages array
            var latestMessage = conversation?.Messages?.LastOrDefault();
            return latestMessage?.Text;
        }
        catch
        {
            return null;
        }
    }
}
