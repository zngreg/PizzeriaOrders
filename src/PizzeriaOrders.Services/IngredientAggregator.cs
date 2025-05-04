using Microsoft.Extensions.Logging;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services;

public interface IIngredientAggregator
{
    Dictionary<string, IngredientItem> Aggregate(IEnumerable<Order> orders);
}

public class IngredientAggregator : IIngredientAggregator
{
    private readonly List<Ingredient> _ingredients;
    private readonly ILogger<IngredientAggregator> _logger;

    public IngredientAggregator(List<Ingredient> ingredients, ILogger<IngredientAggregator> logger)
    {
        _ingredients = ingredients;
        _logger = logger;
    }

    public Dictionary<string, IngredientItem> Aggregate(IEnumerable<Order> orders)
    {
        var totalIngredients = new Dictionary<string, IngredientItem>();

        foreach (var order in orders)
        {
            _logger.LogInformation($"Processing OrderId: {order.OrderId}, DeliverAt: {order.DeliverAt}, CreatedAt: {order.CreatedAt}, CustomerAddress: {order.CustomerAddress}");
            if (order.Products == null)
            {
                _logger.LogWarning($"OrderId: {order.OrderId} has no products.");
                continue;
            }

            foreach (var product in order.Products)
            {
                _logger.LogInformation($"ProductId: {product.ProductId}, Quantity: {product.Quantity}");
                if (product.Quantity <= 0)
                {
                    _logger.LogWarning($"ProductId: {product.ProductId} has invalid quantity: {product.Quantity}");
                    continue;
                }
                var ingredientInfo = _ingredients.FirstOrDefault(i => i.ProductId == product.ProductId);

                if (ingredientInfo != null)
                {
                    _logger.LogInformation($"ProductId: {product.ProductId} has ingredients: {string.Join(", ", ingredientInfo.Ingredients.Select(kvp => $"{kvp.Key}: {kvp.Value.Quantity} {kvp.Value.Units}"))}");
                    foreach (var kvp in ingredientInfo.Ingredients)
                    {
                        _logger.LogInformation($"Processing ingredient: {kvp.Key}, Quantity: {kvp.Value.Quantity} {kvp.Value.Units}");
                        if (totalIngredients.ContainsKey(kvp.Key))
                        {
                            _logger.LogInformation($"Adding {kvp.Value.Quantity * product.Quantity} {kvp.Value.Units} of {kvp.Key} to total ingredients.");
                            totalIngredients[kvp.Key].Quantity += kvp.Value.Quantity * product.Quantity;
                        }
                        else
                        {
                            _logger.LogInformation($"Adding {kvp.Value.Quantity * product.Quantity} {kvp.Value.Units} of {kvp.Key} to total ingredients.");
                            kvp.Value.Quantity = kvp.Value.Quantity * product.Quantity;
                            totalIngredients[kvp.Key] = kvp.Value;
                        }
                        _logger.LogInformation($"Total ingredients now: {string.Join(", ", totalIngredients.Select(kvp => $"{kvp.Key}: {totalIngredients[kvp.Key].Quantity}"))} {totalIngredients[kvp.Key].Units}");
                    }
                    _logger.LogInformation($"Total ingredients after processing product {product.ProductId}: {string.Join(", ", totalIngredients.Select(kvp => $"{kvp.Key}: {totalIngredients[kvp.Key].Quantity} {totalIngredients[kvp.Key].Units}"))}");
                }
                else
                {
                    _logger.LogWarning($"ProductId: {product.ProductId} not found in ingredients.");
                }
            }
        }

        return totalIngredients;
    }
}