using Microsoft.Extensions.Logging;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services;

public interface IPriceCalculator
{
    void CalculatePrice(Order order);
}

public class PriceCalculator : IPriceCalculator
{
    private readonly ILogger<PriceCalculator> _logger;
    private readonly List<Product> _products;

    public PriceCalculator(List<Product> products, ILogger<PriceCalculator> logger)
    {
        _logger = logger;
        _products = products;
    }

    public void CalculatePrice(Order order)
    {
        if (order.Products == null || order.Products.Count() == 0)
        {
            _logger.LogWarning($"OrderId: {order.OrderId} has no products.");
            return;
        }

        var basePrice = 0m;
        var totalVAT = 0m;
        var total = 0m;
        foreach (var product in order.Products)
        {
            _logger.LogInformation($"Calculating price for OrderId: {order.OrderId}, ProductId: {product.ProductId}, Quantity: {product.Quantity}");
            var productDetails = _products.FirstOrDefault(p => p.ProductId == product.ProductId);
            if (productDetails != null)
            {
                _logger.LogInformation($"ProductId: {product.ProductId} found in products list.");
                var productBasePrice = productDetails.Price * product.Quantity;
                _logger.LogInformation($"ProductId: {product.ProductId} base price: {productBasePrice:C2}");
                _logger.LogInformation($"ProductId: {product.ProductId} VAT: {productDetails.VAT}%");
                var VAT = productBasePrice * productDetails.VAT / 100;
                _logger.LogInformation($"ProductId: {product.ProductId} VAT amount: {VAT:C2}");
                product.TotalPrice = productBasePrice + VAT;
                _logger.LogInformation($"ProductId: {product.ProductId} total price including VAT: {product.TotalPrice:C2}");

                basePrice += productBasePrice;
                totalVAT += VAT;
                total += product.TotalPrice;
                _logger.LogInformation($"OrderId: {order.OrderId}, Product: {productDetails.ProductName}, Quantity: {product.Quantity}, Vat for Product: {productDetails.VAT},  Total Price: {product.TotalPrice}");
            }
            else
            {
                _logger.LogWarning($"ProductId: {product.ProductId} not found in products list.");
            }
            _logger.LogInformation($"ProductId: {product.ProductId} has total price: {product.TotalPrice}");
        }

        _logger.LogInformation($"Order Total Price: {basePrice}");
        order.GrossPrice += basePrice;
        order.VATAmount += totalVAT;
        order.TotalPrice += total;
    }
}