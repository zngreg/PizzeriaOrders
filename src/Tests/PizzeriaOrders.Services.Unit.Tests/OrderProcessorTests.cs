using Microsoft.Extensions.Logging;
using Moq;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services.Unit.Tests
{
    [TestFixture]
    public class OrderProcessorTests
    {
        private Mock<IFileParser<Order>> _mockFileParser;
        private Mock<IOrderValidator> _mockValidator;
        private Mock<IPriceCalculator> _mockCalculator;
        private Mock<IIngredientAggregator> _mockAggregator;
        private Mock<IQueueService> _mockQueue;
        private Mock<ILogger<OrderProcessor>> _mockLogger;
        private OrderProcessor _orderProcessor;

        [SetUp]
        public void SetUp()
        {
            _mockFileParser = new Mock<IFileParser<Order>>();
            _mockValidator = new Mock<IOrderValidator>();
            _mockCalculator = new Mock<IPriceCalculator>();
            _mockAggregator = new Mock<IIngredientAggregator>();
            _mockQueue = new Mock<IQueueService>();
            _mockLogger = new Mock<ILogger<OrderProcessor>>();

            _orderProcessor = new OrderProcessor(
                _mockFileParser.Object,
                _mockValidator.Object,
                _mockCalculator.Object,
                _mockAggregator.Object,
                _mockQueue.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public void ProcessOrders_WithValidAndInvalidOrders_ProcessesCorrectly()
        {
            // Arrange
            var validOrder = new Order { OrderId = "1", TotalPrice = 0 };
            var invalidOrder = new Order { OrderId = "2" };

            var orders = new List<Order> { validOrder, invalidOrder };

            _mockValidator.Setup(v => v.IsValid(validOrder)).Returns(new ValidationResult { IsValid = true });
            _mockValidator.Setup(v => v.IsValid(invalidOrder)).Returns(new ValidationResult { IsValid = false, Message = "Invalid order" });

            _mockAggregator.Setup(a => a.Aggregate(It.IsAny<IList<Order>>())).Returns(new Dictionary<string, IngredientItem>());

            // Act
            _orderProcessor.ProcessOrders(orders);

            // Assert
            _mockValidator.Verify(v => v.IsValid(validOrder), Times.Once);
            _mockValidator.Verify(v => v.IsValid(invalidOrder), Times.Once);
            _mockCalculator.Verify(c => c.CalculatePrice(validOrder), Times.Once);
            _mockQueue.Verify(q => q.Push(validOrder), Times.Once);
            _mockLogger.Verify(l => l.Log(
                It.Is<LogLevel>(ll => ll == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        [Test]
        public void ProcessOrders_LogsNumberOfLoadedAndConsolidatedOrders()
        {
            // Arrange
            var orders = new List<Order> { new Order { OrderId = "1" }, new Order { OrderId = "2" } };

            _mockValidator.Setup(v => v.IsValid(orders[0])).Returns(new ValidationResult { IsValid = false, Message = "Invalid order" });
            _mockValidator.Setup(v => v.IsValid(orders[1])).Returns(new ValidationResult { IsValid = false, Message = "Invalid order" });

            // Act
            _orderProcessor.ProcessOrders(orders);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Loaded 2 orders from file.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Consolidated 2 orders.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public void ProcessOrders_NullOrders_ShouldLogAndReturn()
        {
            // Act
            _orderProcessor.ProcessOrders(null);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No orders to process.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public void ProcessOrders_EmptyOrders_ShouldLogAndReturn()
        {
            // Act
            _orderProcessor.ProcessOrders(new List<Order>());

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No orders to process.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public void ProcessOrders_AggregatorReturnsEmpty_ShouldLogEmptyIngredients()
        {
            // Arrange
            var validOrder = new Order { OrderId = "1" };
            var orders = new List<Order> { validOrder };

            _mockValidator.Setup(v => v.IsValid(validOrder)).Returns(new ValidationResult { IsValid = true });
            _mockAggregator.Setup(a => a.Aggregate(It.IsAny<IList<Order>>())).Returns(new Dictionary<string, IngredientItem>());

            // Act
            _orderProcessor.ProcessOrders(orders);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Total ingredients aggregated from valid orders: 0")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public void ProcessOrders_LogsInvalidOrdersDetails()
        {
            // Arrange
            var invalidOrder = new Order { OrderId = "2" };
            var orders = new List<Order> { invalidOrder };

            _mockValidator.Setup(v => v.IsValid(invalidOrder)).Returns(new ValidationResult { IsValid = false, Message = "Invalid order" });

            // Act
            _orderProcessor.ProcessOrders(orders);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order 2 is invalid: Invalid order")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public void ProcessOrders_LogsLoadedAndConsolidatedOrders()
        {
            // Arrange
            var orders = new List<Order> { new Order { OrderId = "1" }, new Order { OrderId = "2" } };

            // Act
            _orderProcessor.ProcessOrders(orders);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Loaded 2 orders from file.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Consolidated 2 orders.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public void ProcessOrders_LogsInvalidOrdersAndReasons()
        {
            // Arrange
            var invalidOrder = new Order { OrderId = "2" };
            var orders = new List<Order> { invalidOrder };

            _mockValidator.Setup(v => v.IsValid(invalidOrder)).Returns(new ValidationResult { IsValid = false, Message = "Invalid order" });

            // Act
            _orderProcessor.ProcessOrders(orders);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order 2 is invalid:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public void ProcessOrders_LogsAggregatedIngredients()
        {
            // Arrange
            var validOrder = new Order { OrderId = "1" };
            var orders = new List<Order> { validOrder };

            _mockValidator.Setup(v => v.IsValid(validOrder)).Returns(new ValidationResult { IsValid = true });
            _mockAggregator.Setup(a => a.Aggregate(It.IsAny<IList<Order>>())).Returns(new Dictionary<string, IngredientItem>
            {
                { "Cheese", new IngredientItem { Quantity = 2, Units = UnitType.Kilograms } }
            });

            // Act
            _orderProcessor.ProcessOrders(orders);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Total ingredients aggregated from valid orders: 1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Ingredient: Cheese, Quantity: 2 Kilograms")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public void ProcessOrders_LogsTotalInvalidOrders()
        {
            // Arrange
            var invalidOrder = new Order { OrderId = "2" };
            var orders = new List<Order> { invalidOrder };

            _mockValidator.Setup(v => v.IsValid(invalidOrder)).Returns(new ValidationResult { IsValid = false, Message = "Invalid order" });

            // Act
            _orderProcessor.ProcessOrders(orders);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Total invalid orders: 1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }
}