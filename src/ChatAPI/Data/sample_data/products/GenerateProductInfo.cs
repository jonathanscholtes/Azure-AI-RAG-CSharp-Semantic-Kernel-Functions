using System.Text;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace ChatAPI.Data;



public class Product
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string description { get; set; } = default!;
}

[JsonSerializable(typeof(List<Product>))]
[JsonSerializable(typeof(Product))]
public partial class ProductJsonContext : JsonSerializerContext
{
}



public sealed class GenerateProductInfo(ILogger<GenerateProductInfo> logger, IConfiguration config, CosmosClient cosmosClient)
{
    private readonly ILogger<GenerateProductInfo> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly string _databaseName = config["CosmosDb_Database"]!;
    private readonly string _containerName = config["CosmosDb_ProductContainer"]!;

    public async Task PopulateCosmosAsync()
    {
        try
        {
            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);
            var container = await database.Database.CreateContainerIfNotExistsAsync(_containerName, "/id");

            var numDocs = 0;

            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
            using (var iterator = container.Container.GetItemQueryIterator<int>(query))
            {
                var result = await iterator.ReadNextAsync();
                numDocs = result.FirstOrDefault();
            }

            if (numDocs == 0)
            {
                _logger.LogInformation("Creating CosmosDB container {ContainerName} in database {DatabaseName}...", _containerName, _databaseName);

            
                var filePath = "./Data/sample_data/products/products.json"; // Path to the single JSON file

                // Read the content of the JSON file
                var content = await File.ReadAllTextAsync(filePath);

                // Deserialize the JSON array using the source generator context
                var products = JsonSerializer.Deserialize(content, ProductJsonContext.Default.ListProduct)!;

                // Iterate through each customer in the array and upsert them
                foreach (var product in products)
                {
                    // Serialize each customer back to a JSON string
                    var productContent = JsonSerializer.Serialize(product, ProductJsonContext.Default.Product);

                    _logger.LogInformation("Product Content: " + productContent);

                    // Create a memory stream from the serialized JSON
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(productContent));

                    // Upsert the item into the container
                    await container.Container.CreateItemStreamAsync(stream, new PartitionKey(product.id));

                    // Log the success message
                    _logger.LogInformation("Upserted item with id {ProductID}", product.id);
                }
            }
            else
            {
                _logger.LogInformation("CosmosDB container already populated, nothing to do.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating Cosmos");
        }
    }
}