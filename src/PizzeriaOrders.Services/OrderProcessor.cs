using Microsoft.Extensions.Logging;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services;

public interface IOrderProcessor
{
    void ProcessOrders(IList<Order> orders);
}

public class OrderProcessor : IOrderProcessor
{
    private readonly IFileParser<Order> _fileParser;
    private readonly IOrderValidator _validator;
    private readonly IPriceCalculator _calculator;
    private readonly IIngredientAggregator _aggregator;
    private readonly IQueueService _queue;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(
        IFileParser<Order> fileParser,
        IOrderValidator validator,
        IPriceCalculator calculator,
        IIngredientAggregator aggregator,
        IQueueService queue,
        ILogger<OrderProcessor> logger)
    {
        _fileParser = fileParser;
        _validator = validator;
        _calculator = calculator;
        _aggregator = aggregator;
        _queue = queue;
        _logger = logger;
    }

    public void ProcessOrders(IList<Order> orders)
    {
        _logger.LogInformation("Starting order processing...");

        if (orders == null || orders.Count == 0)
        {
            _logger.LogWarning("No orders to process.");
            return;
        }

        _logger.LogInformation($"Loaded {orders?.Count} orders from file.");
        var consolidatedOrders = orders?.ConsolidateOrders(_logger);

        if (consolidatedOrders == null)
        {
            _logger.LogWarning("Consolidation returned null. No valid orders to process.");
            return;
        }

        if (consolidatedOrders.Count == 0)
        {
            _logger.LogWarning("No valid orders to process.");
            return;
        }
        _logger.LogInformation($"Consolidated {consolidatedOrders.Count} orders.");

        var validOrders = new List<Order>();
        var invalidOrders = new List<InvalidOrder>();

        _logger.LogInformation($"Validating and processing orders...");
        foreach (var order in consolidatedOrders)
        {
            _logger.LogInformation($"Validating order: {order.OrderId}");
            var validationResult = _validator.IsValid(order);
            _logger.LogInformation($"Validation result for order {order?.OrderId}: {validationResult?.IsValid}");
            if (validationResult != null && validationResult.IsValid)
            {
                _logger.LogInformation($"Calculating price for order: {order?.OrderId}");
                _calculator.CalculatePrice(order);
                _logger.LogInformation($"Price calculated for order {order.OrderId}: {order.TotalPrice}");
                _logger.LogInformation($"Order {order.OrderId} is valid.");
                validOrders.Add(order);
                _logger.LogInformation($"Order {order.OrderId} is valid and will be pushed to queue.");
                _logger.LogInformation($"Pushing order {order.OrderId} to queue.");
                _queue.Push(order);
                _logger.LogInformation($"Order {order.OrderId} pushed to queue successfully.");
            }
            else
            {
                _logger.LogWarning($"Order {order?.OrderId} is invalid: {validationResult?.Message}");
                _logger.LogInformation($"Adding invalid order {order?.OrderId} to invalid orders list.");
                invalidOrders.Add(
                    new InvalidOrder
                    {
                        Order = order,
                        Reason = validationResult?.Message ?? "Unknown validation error"
                    }
                );
                _logger.LogInformation($"Invalid order {order.OrderId} added to invalid orders list.");
            }
            _logger.LogInformation($"Order {order.OrderId} processed.");
        }
        _logger.LogInformation($"Finished processing orders.");


        _logger.LogInformation($"#############################################################################");

        _logger.LogInformation($"Total valid orders: {validOrders?.Count}");
        _logger.LogInformation($"Gross Price for Valid Orders: {validOrders?.Sum(o => o.GrossPrice):C2}");
        _logger.LogInformation($"Total VAT Amount for Valid Orders: {validOrders?.Sum(o => o.VATAmount):C2}");
        _logger.LogInformation($"Total Price for Valid Orders: {validOrders?.Sum(o => o.TotalPrice):C2}");

        var totalIngredients = _aggregator.Aggregate(validOrders);
        if (totalIngredients != null || totalIngredients?.Count > 0)
        {
            _logger.LogInformation($"******************************************************************************");
            _logger.LogInformation($"Aggregating ingredients from valid orders...");
            _logger.LogInformation($"Total ingredients aggregated from valid orders: {totalIngredients?.Count}");
            _logger.LogInformation($"----------------------------------------------------------------------------");
            _logger.LogInformation($"Ingredients:");
            foreach (var ingredient in totalIngredients)
            {
                _logger.LogInformation($"Ingredient: {ingredient.Key}, Quantity: {ingredient.Value.Quantity} {ingredient.Value.Units}");
            }
        }

        _logger.LogInformation($"******************************************************************************");

        _logger.LogInformation($"Total invalid orders: {invalidOrders.Count}");
        _logger.LogInformation($"----------------------------------------------------------------------------");

        _logger.LogInformation($"Invalid Orders:");
        foreach (var invalidOrder in invalidOrders)
        {
            if (invalidOrder?.Order == null)
            {
                _logger.LogWarning("Invalid order entry is null or missing order details.");
                continue;
            }
            _logger.LogInformation($"Order ID: {invalidOrder.Order.OrderId}, Reason: {invalidOrder.Reason}");
        }

        _logger.LogInformation($"#############################################################################");
    }
}