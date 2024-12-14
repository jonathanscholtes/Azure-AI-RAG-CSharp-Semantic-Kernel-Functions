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


public sealed class ChatService(Kernel kernel, ITextEmbeddingGenerationService embedding, ProductData productData,ChatHistory chatHistory, AISearchData aiSearch, ILogger<ChatService> logger)
{
    private readonly ProductData _productData = productData;
    private readonly AISearchData _aiSearch = aiSearch;
    private readonly ILogger<ChatService> _logger = logger;

    private readonly Kernel _kernel = kernel;
    private readonly ITextEmbeddingGenerationService _embedding = embedding;

    private readonly ChatHistory _chatHistory = chatHistory;

    public async Task<string> GetResponseAsync( string question)
    {
         _chatHistory.AddUserMessage(question);

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
        _chatHistory.AddAssistantMessage(resp);

        return JsonSerializer.Serialize(new { resp });
 
    }


    private async Task<byte[]> GetImagesAsBytes(string url)
    {       
            var blobClient = new BlobClient(new Uri(url),new DefaultAzureCredential());
            using var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            return stream.ToArray();
       
    }
    private async Task<List<string>> GetImagesAsBase64Async(List<string> imageBlobUrls)
    {
        var tasks = imageBlobUrls.Select(async url =>
        {
            var blobClient = new BlobClient(new Uri(url),new DefaultAzureCredential());
            using var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
        });

        var base64Images = await Task.WhenAll(tasks);
        return base64Images.ToList();
    }
}