using Microsoft.Extensions.Logging;
using Moq;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services.Unit.Tests
{
    [TestFixture]
    public class OrderExtensionsTests
    {
        private Mock<ILogger> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Test]
        public void ConsolidateOrders_ShouldConsolidateOrdersWithSameOrderId()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = "1",
                    DeliverAt = DateTime.Parse("2025-05-04"),
                    CreatedAt = DateTime.Parse("2025-05-03"),
                    CustomerAddress = "123 Pizza St",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "P1", Quantity = 1 }
                    }
                },
                new Order
                {
                    OrderId = "1",
                    DeliverAt = DateTime.Parse("2025-05-04"),
                    CreatedAt = DateTime.Parse("2025-05-03"),
                    CustomerAddress = "123 Pizza St",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "P1", Quantity = 2 },
                        new OrderProduct { ProductId = "P2", Quantity = 1 }
                    }
                }
            };

            // Act
            var consolidatedOrders = orders.ConsolidateOrders(_mockLogger.Object);

            // Assert
            Assert.That(1, Is.EqualTo(consolidatedOrders.Count));
            var consolidatedOrder = consolidatedOrders.First();
            Assert.That(2, Is.EqualTo(consolidatedOrder.Products.Count));
            Assert.That(3, Is.EqualTo(consolidatedOrder.Products.First(p => p.ProductId == "P1").Quantity));
            Assert.That(1, Is.EqualTo(consolidatedOrder.Products.First(p => p.ProductId == "P2").Quantity));
        }

        [Test]
        public void ConsolidateOrders_ShouldNotConsolidateOrdersWithDifferentFields()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = "1",
                    DeliverAt = DateTime.Parse("2025-05-04"),
                    CreatedAt = DateTime.Parse("2025-05-03"),
                    CustomerAddress = "123 Pizza St",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "P1", Quantity = 1 }
                    }
                },
                new Order
                {
                    OrderId = "1",
                    DeliverAt = DateTime.Parse("2025-05-05"),
                    CreatedAt = DateTime.Parse("2025-05-03"),
                    CustomerAddress = "123 Pizza St",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "P2", Quantity = 1 }
                    }
                }
            };

            // Act
            var consolidatedOrders = orders.ConsolidateOrders(_mockLogger.Object);

            // Assert
            Assert.That(2, Is.EqualTo(consolidatedOrders.Count));
        }

        [Test]
        public void ConsolidateOrders_ShouldHandleEmptyOrdersList()
        {
            // Arrange
            var orders = new List<Order>();

            // Act
            var consolidatedOrders = orders.ConsolidateOrders(_mockLogger.Object);

            // Assert
            Assert.That(consolidatedOrders, Is.Empty);
        }

        [Test]
        public void ConsolidateOrders_ShouldAddNewProductIfNotExists()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = "1",
                    DeliverAt = DateTime.Parse("2025-05-04"),
                    CreatedAt = DateTime.Parse("2025-05-03"),
                    CustomerAddress = "123 Pizza St",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "P1", Quantity = 1 }
                    }
                },
                new Order
                {
                    OrderId = "1",
                    DeliverAt = DateTime.Parse("2025-05-04"),
                    CreatedAt = DateTime.Parse("2025-05-03"),
                    CustomerAddress = "123 Pizza St",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "P2", Quantity = 1 }
                    }
                }
            };

            // Act
            var consolidatedOrders = orders.ConsolidateOrders(_mockLogger.Object);

            // Assert
            Assert.That(1, Is.EqualTo(consolidatedOrders.Count));
            var consolidatedOrder = consolidatedOrders.First();
            Assert.That(2, Is.EqualTo(consolidatedOrder.Products.Count));
            Assert.That(1, Is.EqualTo(consolidatedOrder.Products.First(p => p.ProductId == "P2").Quantity));
        }
    }
}