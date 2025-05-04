using Microsoft.Extensions.Logging;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services;

public interface IQueueService
{
    void Push(Order order);
    List<Order> GetAll();
}

public class MockQueueService : IQueueService
{
    private readonly List<Order> _queue = new();
    private readonly ILogger<MockQueueService> _logger;

    public MockQueueService(ILogger<MockQueueService> logger)
    {
        _logger = logger;
    }

    public void Push(Order order)
    {
        if (order == null)
        {
            _logger.LogWarning("Attempted to add a null order to the queue.");
            return;
        }

        _logger.LogInformation($"Adding OrderId: {order.OrderId}, DeliverAt: {order.DeliverAt}, CreatedAt: {order.CreatedAt}, CustomerAddress: {order.CustomerAddress} to the queue.");
        _queue.Add(order);
        _logger.LogInformation($"OrderId: {order.OrderId} added to the queue. Total orders in queue: {_queue.Count}");
    }

    public List<Order> GetAll() => _queue;
}