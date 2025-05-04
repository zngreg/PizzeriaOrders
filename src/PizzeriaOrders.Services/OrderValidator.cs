using Microsoft.Extensions.Logging;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services;

public interface IOrderValidator
{
    ValidationResult IsValid(Order order);
}

public class OrderValidator : IOrderValidator
{
    private readonly HashSet<string> _validProductIds;
    private readonly ILogger<OrderValidator> _logger;
    HashSet<string> orderIdSet;

    public OrderValidator(List<Product> products, ILogger<OrderValidator> logger)
    {
        _validProductIds = products?.Select(p => p.ProductId).ToHashSet() ?? new HashSet<string>();
        _logger = logger;
    }

    public ValidationResult IsValid(Order order)
    {
        if (order == null)
        {
            _logger.LogWarning("Order is null.");

            return new ValidationResult
            {
                IsValid = false,
                Message = "Order is null."
            };
        }

        orderIdSet ??= new HashSet<string>();
        if (!orderIdSet.Add(order.OrderId))
        {
            _logger.LogWarning($"Duplicate OrderId found: {order.OrderId}");
            return new ValidationResult
            {
                IsValid = false,
                Message = $"Duplicate OrderId found: {order.OrderId}"
            };
        }

        if (string.IsNullOrWhiteSpace(order.OrderId))
        {
            _logger.LogWarning("Order ID is null or empty.");
            return new ValidationResult
            {
                IsValid = false,
                Message = "Order ID is null or empty."
            };
        }

        if (order.Products == null || !order.Products.Any())
        {
            _logger.LogWarning("Products list is null or empty.");
            return new ValidationResult
            {
                IsValid = false,
                Message = "Products list is null or empty."
            };
        }

        if (order.Products.Any(p => string.IsNullOrWhiteSpace(p.ProductId) || !_validProductIds.Contains(p.ProductId)))
        {
            _logger.LogWarning("One or more product IDs in the order are invalid.");
            return new ValidationResult
            {
                IsValid = false,
                Message = "One or more product IDs in the order are invalid."
            };
        }

        if (order.Products.Any(p => p.Quantity <= 0))
        {
            _logger.LogWarning("One or more product quantities in the order are invalid.");
            return new ValidationResult
            {
                IsValid = false,
                Message = "One or more product quantities in the order are invalid."
            };
        }

        if (order.DeliverAt <= order.CreatedAt || order.DeliverAt <= DateTime.Now)
        {
            _logger.LogWarning($"Delivery time '{order.DeliverAt}' is invalid.");
            return new ValidationResult
            {
                IsValid = false,
                Message = $"Delivery time '{order.DeliverAt}' is invalid."
            };
        }

        if (order.CreatedAt > DateTime.Now)
        {
            _logger.LogWarning($"Order creation time '{order.CreatedAt}' is in the future.");
            return new ValidationResult
            {
                IsValid = false,
                Message = $"Order creation time '{order.CreatedAt}' is in the future."
            };
        }

        if (string.IsNullOrWhiteSpace(order.CustomerAddress))
        {
            _logger.LogWarning("Customer address is null or empty.");
            return new ValidationResult
            {
                IsValid = false,
                Message = "Customer address is null or empty."
            };
        }

        _logger.LogInformation($"Order '{order.OrderId}' is valid.");

        return new ValidationResult
        {
            IsValid = true,
        };
    }
}