using Microsoft.SemanticKernel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChatAPI.Data;
using Microsoft.SemanticKernel.ChatCompletion;
using Azure.Storage.Blobs;
using Azure.Identity;
using System.ComponentModel;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;

namespace ChatAPI.Plugins;

public class AISearchDataPlugin
{
    private readonly AISearchData _aisearchData;
    private readonly ILogger<AISearchDataPlugin> _logger;

    private readonly ITextEmbeddingGenerationService _embedding;


    public AISearchDataPlugin(AISearchData aisearchData, ITextEmbeddingGenerationService embedding, ILogger<AISearchDataPlugin> logger)
    {
        _aisearchData = aisearchData;
        _logger = logger;
        _embedding = embedding;
       
    }

    [KernelFunction("troubleshoot_lookup")]
    [Description("lookup resources to troubleshoot user technical questions")]
    public async Task<List<Dictionary<string, string?>>> ResourceLookup(string question)
    {
        try
        {   var embeddingTask = _embedding.GenerateEmbeddingAsync(question);
             var embedding = await embeddingTask;
             string embeddingString = JsonSerializer.Serialize(embedding);
            
            var context = await _aisearchData.RetrieveDocumentationAsync(question, embedding);
            

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Error retrieving aisearch data: {question}", question);
            throw;
        }
    }
}