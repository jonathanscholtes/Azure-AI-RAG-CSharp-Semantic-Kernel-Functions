using Microsoft.SemanticKernel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChatAPI.Data;
using Microsoft.SemanticKernel.ChatCompletion;
using Azure.Storage.Blobs;
using Azure.Identity;
using System.ComponentModel;

namespace ChatAPI.Plugins;

public class ProductDataPlugin
{
    private readonly ProductData _productData;
    private readonly ILogger<ProductDataPlugin> _logger;


    public ProductDataPlugin(ProductData productData, ILogger<ProductDataPlugin> logger)
    {
        _productData = productData;
        _logger = logger;
       
    }

    [KernelFunction("product_lookup")]
    [Description("Look up and return addition information for product using product id.")]
    public async Task<string> GetProduct(string product_id)
    {
        try
        {   
            
            _logger.LogInformation("Retrieving product data for ProductId: {ProductId}", product_id);
            var product = await _productData.GetProductAsync(product_id);
            var productJson = System.Text.Json.JsonSerializer.Serialize(product);
            _logger.LogInformation("Product data retrieved: {ProductData}", productJson);
            

            return productJson;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Error retrieving product data for ProductId: {ProductId}", product_id);
            throw;
        }
    }

   /*  [KernelFunction("find_product_by_name")]
    [Description("Look up and return addition information for Azure products and service using product name.")]
    public async Task<string> GetProductDataByNameAsync(string product_name)
    {
        try
        {   
            
            _logger.LogInformation("Retrieving product data for ProductName: {ProductName}", product_name);
            var product = await _productData.GetProductByNameAsync(product_name);
            var productJson = System.Text.Json.JsonSerializer.Serialize(product);
            _logger.LogInformation("Product data retrieved: {ProductData}", productJson);
            

            return productJson;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Error retrieving product data for ProductName: {ProductName}", product_name);
            throw;
        }
    }*/
}