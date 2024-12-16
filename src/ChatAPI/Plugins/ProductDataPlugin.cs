using Microsoft.SemanticKernel;
using ChatAPI.Data;
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

    [KernelFunction("get_azure_product_by_id")]
    [Description("Retrieve detailed metadata and information about a specific Azure product or service using its unique product ID. This function is helpful for understanding features, pricing, and documentation links related to the Azure product.")]
    [return: Description("detailed information about the specified Azure product.")]
    public async Task<string> GetAzureProductDetailsById([Description("The unique identifier (ID) of the Azure product or service. Example: '123' or '127'.")]string product_id)
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
}

    /*[KernelFunction("lookup_azure_product_by_name")]
    [Description("Look up and return addition information for Azure products and service using product name.")]
    [return: Description("A detail description on an Azure product")]
    public async Task<string> GetProductDataByName([Description("The Name of the product")]string product_name)
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
