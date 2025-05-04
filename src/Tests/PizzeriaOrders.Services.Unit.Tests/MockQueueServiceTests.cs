using Microsoft.Extensions.Logging;
using Moq;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services.Unit.Tests;

[TestFixture]
public class MockQueueServiceTests
{
    private Mock<ILogger<MockQueueService>> _mockLogger;
    private MockQueueService _queueService;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<MockQueueService>>();
        _queueService = new MockQueueService(_mockLogger.Object);
    }

    [Test]
    public void Push_NullOrder_LogsWarningAndDoesNotAddToQueue()
    {
        // Act
        _queueService.Push(null);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempted to add a null order to the queue.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        Assert.That(_queueService.GetAll(), Is.Empty);
    }

    [Test]
    public void Push_ValidOrder_LogsInformationAndAddsToQueue()
    {
        // Arrange
        var order = new Order
        {
            OrderId = "1",
            DeliverAt = DateTime.Parse("2025-05-05T10:00:00"),
            CreatedAt = DateTime.Parse("2025-05-04T10:00:00"),
            CustomerAddress = "123 Pizza Street"
        };

        // Act
        _queueService.Push(order);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding OrderId: 1")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);

        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("OrderId: 1 added to the queue.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);

        var queue = _queueService.GetAll();
        Assert.That(1, Is.EqualTo(queue.Count));
        Assert.That(order, Is.EqualTo(queue[0]));
    }

    [Test]
    public void GetAll_ReturnsAllOrdersInQueue()
    {
        // Arrange
        var order1 = new Order { OrderId = "1" };
        var order2 = new Order { OrderId = "2" };

        _queueService.Push(order1);
        _queueService.Push(order2);

        // Act
        var result = _queueService.GetAll();

        // Assert
        Assert.That(2, Is.EqualTo(result.Count));
        Assert.That(order1, Is.AnyOf(result));
        Assert.That(order2, Is.AnyOf(result));
    }
}