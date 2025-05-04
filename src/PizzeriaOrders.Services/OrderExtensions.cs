using Microsoft.Extensions.Logging;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services;

public static class OrderExtensions
{
    public static List<Order> ConsolidateOrders(this IEnumerable<Order> orders, ILogger logger)
    {
        var groupedOrders = orders.GroupBy(o => o.OrderId);
        var consolidatedOrders = new List<Order>();

        foreach (var group in groupedOrders)
        {
            logger.LogInformation($"Consolidating orders for Order ID: {group.Key}");
            var firstOrder = group.First();
            logger.LogInformation($"First order details: {firstOrder}");

            foreach (var order in group.Skip(1))
            {
                logger.LogInformation($"Processing order for consolidation: {order}");

                if (order.DeliverAt != firstOrder.DeliverAt ||
                    order.CreatedAt != firstOrder.CreatedAt ||
                    order.CustomerAddress != firstOrder.CustomerAddress)
                {
                    logger.LogWarning($"Order '{order.OrderId}' has mismatched fields and cannot be consolidated.");
                    logger.LogWarning($"DeliverAt: {order.DeliverAt} vs {firstOrder.DeliverAt}");
                    logger.LogWarning($"CreatedAt: {order.CreatedAt} vs {firstOrder.CreatedAt}");
                    logger.LogWarning($"CustomerAddress: {order.CustomerAddress} vs {firstOrder.CustomerAddress}");
                    logger.LogWarning($"Order '{order.OrderId}' will not be consolidated.");

                    logger.LogWarning($"Adding order '{order.OrderId}' to consolidated orders as a separate entry.");
                    consolidatedOrders.Add(order);
                    logger.LogInformation($"Order '{order.OrderId}' added as a separate entry.");

                    continue;
                }

                if (order.Products == null || order.Products.Count == 0)
                {
                    logger.LogWarning($"Order '{order.OrderId}' has no products and skipping this order.");
                    continue;
                }

                foreach (var product in order.Products)
                {
                    logger.LogInformation($"Consolidating product '{product.ProductId}' from order '{order.OrderId}' into order '{firstOrder.OrderId}'.");
                    var existingProduct = firstOrder?.Products?.FirstOrDefault(p => p.ProductId == product.ProductId);
                    if (existingProduct != null)
                    {
                        logger.LogInformation($"Product '{product.ProductId}' already exists in order '{firstOrder.OrderId}'. Updating quantity.");
                        existingProduct.Quantity += product.Quantity;
                        logger.LogInformation($"Updated quantity for product '{product.ProductId}' to {existingProduct.Quantity}.");
                    }
                    else
                    {
                        logger.LogInformation($"Product '{product.ProductId}' does not exist in order '{firstOrder.OrderId}'. Adding new product.");
                        firstOrder.Products ??= [];
                        firstOrder.Products.Add(product);
                        logger.LogInformation($"Added new product '{product.ProductId}' with quantity {product.Quantity} to order '{firstOrder.OrderId}'.");
                    }
                    logger.LogInformation($"Product '{product.ProductId}' consolidated successfully.");
                }
                logger.LogInformation($"Order '{order.OrderId}' consolidated into order '{firstOrder.OrderId}'.");
            }
            logger.LogInformation($"Finalizing order '{firstOrder.OrderId}' with consolidated products.");

            consolidatedOrders.Add(firstOrder);
            logger.LogInformation($"Order '{firstOrder.OrderId}' consolidated successfully.");
        }
        logger.LogInformation($"Total consolidated orders: {consolidatedOrders.Count}");

        return consolidatedOrders;
    }
}