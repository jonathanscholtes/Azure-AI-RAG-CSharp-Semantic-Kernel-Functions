using ChatAPI.Data;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Identity;
using ChatAPI.Plugins;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace ChatAPI.Services;


public sealed class ChatService(Kernel kernel, ITextEmbeddingGenerationService embedding, ProductData productData,ChatHistory chatHistory,ChatHistoryData chatHistoryData, AISearchData aiSearch, ILogger<ChatService> logger)
{
    private readonly ProductData _productData = productData;
    private readonly AISearchData _aiSearch = aiSearch;
    private readonly ILogger<ChatService> _logger = logger;

    private readonly Kernel _kernel = kernel;
    private readonly ITextEmbeddingGenerationService _embedding = embedding;

    private readonly ChatHistoryData _chatHistoryData = chatHistoryData;

     private readonly ChatHistory _chatHistory = chatHistory;
    


    public async Task<string> GetResponseAsync( string question, string sessionId)
    {
         _logger.LogInformation("Chat History Count {count}",chatHistory.Count );

        if(_chatHistory.Count ==1)
        {
            _logger.LogInformation("Init Chat History");
            await _chatHistoryData.InitializeChatHistoryFromCosmosDBAsync(_chatHistory,sessionId);
        }

        await _chatHistoryData.AddMessageToHistoryAndSaveAsync(_chatHistory,sessionId,"user",question);

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() 
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        
        ChatMessageContent  response  = await  chatCompletion.GetChatMessageContentAsync(
        _chatHistory,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

        string resp = string.Join(" ",response.Items);
        _logger.LogInformation("Response {response}",resp );
        await _chatHistoryData.AddMessageToHistoryAndSaveAsync(_chatHistory,sessionId,"assistant",resp);

        return JsonSerializer.Serialize(new { resp });
 
    }


}