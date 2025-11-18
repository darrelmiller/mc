using CopilotCli.Services;

namespace CopilotCli.Cli;

/// <summary>
/// Handles one-shot query execution.
/// </summary>
public class OneShotCommand
{
    private readonly IAuthProvider _authProvider;
    private readonly ICopilotClient _copilotClient;

    public OneShotCommand(IAuthProvider authProvider, ICopilotClient copilotClient)
    {
        _authProvider = authProvider;
        _copilotClient = copilotClient;
    }

    /// <summary>
    /// Executes a one-shot query and streams the response to stdout.
    /// </summary>
    /// <param name="query">The query text to send.</param>
    /// <param name="useStreaming">Whether to use streaming endpoint.</param>
    /// <returns>Exit code (0 for success, non-zero for errors).</returns>
    public async Task<int> ExecuteAsync(string query, bool useStreaming = false)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(query))
            {
                ErrorHandler.WriteError("Message cannot be empty", ErrorHandler.InvalidInput);
                return ErrorHandler.InvalidInput;
            }


            // Create conversation
            var conversation = await _copilotClient.CreateConversationAsync();
            if (conversation?.Id == null)
            {
                ErrorHandler.WriteError("Failed to create conversation", ErrorHandler.ConversationError);
                return ErrorHandler.ConversationError;
            }

            if (useStreaming)
            {
                // Send message and get streaming response
                var response = await _copilotClient.SendMessageAsync(conversation.Id.ToString()!, query);

                // Parse and display SSE stream
                await foreach (var sseEvent in StreamParser.ParseSseStreamAsync(response))
                {
                    if (!string.IsNullOrEmpty(sseEvent.Data))
                    {
                        var messageText = StreamParser.ExtractMessageText(sseEvent.Data);
                        if (!string.IsNullOrEmpty(messageText))
                        {
                            Console.WriteLine(messageText);
                        }
                    }
                }
            }
            else
            {
                // Send message and get non-streaming response
                var updatedConversation = await _copilotClient.SendMessageNonStreamingAsync(conversation.Id.ToString()!, query);
                
                // Get the latest message from the conversation
                if (updatedConversation?.Messages != null && updatedConversation.Messages.Count > 0)
                {
                    var latestMessage = updatedConversation.Messages[updatedConversation.Messages.Count - 1];
                    if (latestMessage?.Text != null)
                    {
                        Console.WriteLine(latestMessage.Text);
                    }
                }
            }

            return ErrorHandler.Success;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("login"))
        {
            // Not authenticated or token expired
            ErrorHandler.WriteError(ex.Message, ErrorHandler.AuthError);
            return ErrorHandler.AuthError;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Authentication"))
        {
            // Authentication error
            ErrorHandler.WriteError(ex.Message, ErrorHandler.AuthError);
            return ErrorHandler.AuthError;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Forbidden"))
        {
            // Permission denied error
            ErrorHandler.WriteError(ex.Message, ErrorHandler.PermissionDenied);
            return ErrorHandler.PermissionDenied;
        }
        catch (HttpRequestException ex)
        {
            // Network error
            ErrorHandler.WriteError($"Network error: {ex.Message}", ErrorHandler.NetworkError);
            return ErrorHandler.NetworkError;
        }
        catch (Exception ex)
        {
            // General error
            ErrorHandler.WriteError($"Unexpected error: {ex.Message}", ErrorHandler.ConversationError);
            return ErrorHandler.ConversationError;
        }
    }
}
