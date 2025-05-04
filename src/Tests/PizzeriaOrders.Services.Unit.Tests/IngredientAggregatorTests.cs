using Microsoft.Extensions.Logging;
using Moq;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services.Unit.Tests
{
    [TestFixture]
    public class IngredientAggregatorTests
    {
        private Mock<ILogger<IngredientAggregator>> _loggerMock;
        private List<Ingredient> _ingredients;
        private IngredientAggregator _aggregator;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<IngredientAggregator>>();
            _ingredients = new List<Ingredient>
            {
                new Ingredient
                {
                    ProductId = "1",
                    Ingredients = new Dictionary<string, IngredientItem>
                    {
                        { "Cheese", new IngredientItem { Quantity = 0.01m, Units = UnitType.Kilograms } },
                        { "Tomato", new IngredientItem { Quantity = 0.05m, Units = UnitType.Kilograms } }
                    }
                },
                new Ingredient
                {
                    ProductId = "2",
                    Ingredients = new Dictionary<string, IngredientItem>
                    {
                        { "Dough", new IngredientItem { Quantity = 0.1m, Units = UnitType.Kilograms } }
                    }
                }
            };
            _aggregator = new IngredientAggregator(_ingredients, _loggerMock.Object);
        }

        [Test]
        public void Aggregate_ShouldReturnEmptyDictionary_WhenOrdersAreEmpty()
        {
            var orders = new List<Order>();

            var result = _aggregator.Aggregate(orders);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Aggregate_ShouldLogWarning_WhenOrderHasNoProducts()
        {
            var orders = new List<Order>
            {
                new Order { OrderId = "1", Products = null }
            };

            var result = _aggregator.Aggregate(orders);

            Assert.That(result, Is.Empty);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("has no products")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public void Aggregate_ShouldLogWarning_WhenProductQuantityIsInvalid()
        {
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = "1",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "1", Quantity = 0 }
                    }
                }
            };

            var result = _aggregator.Aggregate(orders);

            Assert.That(result, Is.Empty);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("has invalid quantity")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public void Aggregate_ShouldLogWarning_WhenProductNotFoundInIngredients()
        {
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = "1",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "3", Quantity = 1 }
                    }
                }
            };

            var result = _aggregator.Aggregate(orders);

            Assert.That(result, Is.Empty);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not found in ingredients")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public void Aggregate_ShouldAggregateIngredientsCorrectly_WhenValidOrdersProvided()
        {
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = "1",
                    Products = new List<OrderProduct>
                    {
                        new OrderProduct { ProductId = "1", Quantity = 2 },
                        new OrderProduct { ProductId = "2", Quantity = 1 }
                    }
                }
            };

            var result = _aggregator.Aggregate(orders);

            Assert.That(3, Is.EqualTo(result.Count));
            Assert.That(0.02m, Is.EqualTo(result["Cheese"].Quantity));
            Assert.That(0.1m, Is.EqualTo(result["Tomato"].Quantity));
            Assert.That(0.1m, Is.EqualTo(result["Dough"].Quantity));
        }
    }
}