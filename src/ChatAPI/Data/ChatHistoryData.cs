using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ChatAPI.Data;

public sealed class ChatHistoryData(CosmosClient cosmosClient, ILogger<ProductData> logger, IConfiguration config)
{
    // Assume a Cosmos DB client is injected for operations
     private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly ILogger<ProductData> logger = logger;
    private readonly string _databaseName = config["CosmosDb_Database"]!;

    private readonly string _containerName = config["CosmosDb_ChatContainer"]!;
   

    public async Task SaveChatAsync(string productId, string question, string? answer)
    {
        // Save the question-answer pair into Cosmos DB with productId and timestamp
        //var chatRecord = new {SessionId=Guid.NewGuid().ToString(), productId, question, answer, timestamp = DateTime.UtcNow };

        var chatRecord = new
        {
            id = Guid.NewGuid().ToString(), 
            sessionId = Guid.NewGuid().ToString(),
            ProductId = productId,
            Question = question,
            Answer = answer,
            Timestamp = DateTime.UtcNow
        };

        var container = _cosmosClient.GetContainer(_databaseName, _containerName);
        //await _cosmosClient.SaveAsync(chatRecord);  // Implement SaveAsync based on your Cosmos DB setup

        PartitionKey partitionKey = new PartitionKey(chatRecord.sessionId);
        await container.CreateItemAsync(
            item: chatRecord,
            partitionKey: partitionKey
        );
    }
}
