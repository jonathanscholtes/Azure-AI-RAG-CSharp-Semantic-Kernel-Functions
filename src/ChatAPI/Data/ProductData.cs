using Microsoft.Azure.Cosmos;

namespace ChatAPI.Data;

public sealed class ProductData(CosmosClient cosmosClient, ILogger<ProductData> logger, IConfiguration config)
{
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly ILogger<ProductData> logger = logger;
    private readonly string _databaseName = config["CosmosDb_Database"]!;
    private readonly string _containerName = config["CosmosDb_ProductContainer"]!;

    public async Task<Dictionary<string, object>> GetProductAsync(string productId)
    {
        try
        {
            var container = _cosmosClient.GetContainer(_databaseName, _containerName);

            var response = await container.ReadItemAsync<Dictionary<string, object>>(productId, new PartitionKey(productId));
            var product = response.Resource;

            // Limit orders to the first 2 items
            //if (product.TryGetValue("orders", out object? orders) && orders is List<object> list)
            //{
            //    product["orders"] = list.Take(2).ToList();
            //}

            return product;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogError("Product with ID {ProductID} not found.", productId);
            throw;
        }
    }

     public async Task<Dictionary<string, object>> GetProductByNameAsync(string productName)
    {
        try
        {
            var container = _cosmosClient.GetContainer(_databaseName, _containerName);

             // Define the query to search by ProductName
        var query = new QueryDefinition("SELECT * FROM c WHERE c.name = @productName")
            .WithParameter("@productName", productName);

        var iterator = container.GetItemQueryIterator<Dictionary<string, object>>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();

            if (response.Resource.Count() > 0)
            {
                // Assuming the first match is returned
                return response.Resource.First();
            }
        }

        logger.LogWarning("No product found with name: {ProductName}", productName);
        return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogError("Product with ID {productName} not found.", productName);
            throw;
        }
    }
}