using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Linq;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ChatAPI.Data;


public class ChatMessage
{   
    public string id { get; set; }
    public string sessionid { get; set; }
    public string message { get; set; }
    public string role { get; set; } // "User" or "Assistant"
    public DateTime Timestamp { get; set; }
}

public sealed class ChatHistoryData(CosmosClient cosmosClient, ILogger<ProductData> logger, IConfiguration config)
{
    // Assume a Cosmos DB client is injected for operations
     private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly ILogger<ProductData> logger = logger;
    private readonly string _databaseName = config["CosmosDb_Database"]!;

    private readonly string _containerName = config["CosmosDb_ChatContainer"]!;
   

    private async Task AddUserMessageAsync(string sessionId, string message)
    {
        var chatMessage = new ChatMessage
        {
            id = Guid.NewGuid().ToString(), 
            sessionid = sessionId,
            message = message,
            role = "user",
            Timestamp = DateTime.UtcNow
        };

        var container = _cosmosClient.GetContainer(_databaseName, _containerName);
        await container.CreateItemAsync(chatMessage, new PartitionKey(chatMessage.id));
    }

    private async Task AddAssistantMessageAsync(string sessionId, string message)
    {
        var chatMessage = new ChatMessage
        {
            id = Guid.NewGuid().ToString(), 
            sessionid = sessionId,
            message = message,
            role = "assistant",
            Timestamp = DateTime.UtcNow
        };
        var container = _cosmosClient.GetContainer(_databaseName, _containerName);
        await container.CreateItemAsync(chatMessage, new PartitionKey(chatMessage.id));
    }

    public async Task AddMessageToHistoryAndSaveAsync(ChatHistory chatHistory, string sessionId, string role, string content)
{
    // Add message to ChatHistory
    if (role == "user")
    {
        chatHistory.AddUserMessage(content);
        await AddUserMessageAsync(sessionId,content);
    }
    else if (role == "assistant")
    {
        chatHistory.AddAssistantMessage(content);
        await AddAssistantMessageAsync(sessionId,content);
    }

}
    public async Task InitializeChatHistoryFromCosmosDBAsync(ChatHistory chatHistory, string sessionId)
{
    var messages = await GetMessagesBySessionIdAsync(sessionId);

    foreach (var message in messages)
    {
        if (message.role == "user")
        {
            chatHistory.AddUserMessage(message.message);
        }
        else if (message.role == "assistant")
        {
            chatHistory.AddAssistantMessage(message.message);
        }
    }

}

    private async Task<List<ChatMessage>> GetMessagesBySessionIdAsync(string sessionId)
    {
        var container = _cosmosClient.GetContainer(_databaseName, _containerName);
        var query = container.GetItemQueryIterator<ChatMessage>(
            new QueryDefinition("SELECT * FROM c WHERE c.sessionid = @sessionId")
            .WithParameter("@sessionId", sessionId)
        );

       var messages = new List<ChatMessage>();

        // Iterate through pages of results
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            messages.AddRange(response); // Add results from the current page
        }

        return messages;
    }
}

