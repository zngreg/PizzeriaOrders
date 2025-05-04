using Microsoft.Extensions.Logging;
using Moq;
using PizzeriaOrders.Models;

namespace PizzeriaOrders.Services.Unit.Tests;

[TestFixture]
public class PriceCalculatorTests
{
    private Mock<ILogger<PriceCalculator>> _loggerMock;
    private List<Product> _products;
    private PriceCalculator _priceCalculator;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PriceCalculator>>();
        _products = new List<Product>
        {
            new Product { ProductId = "1", ProductName = "Pizza Margherita", Price = 10.0m, VAT = 15 },
            new Product { ProductId = "2", ProductName = "Pizza Pepperoni", Price = 12.0m, VAT = 15 }
        };
        _priceCalculator = new PriceCalculator(_products, _loggerMock.Object);
    }

    [Test]
    public void CalculatePrice_OrderWithNoProducts_LogsWarning()
    {
        var order = new Order { OrderId = "1", Products = new List<OrderProduct>() };

        _priceCalculator.CalculatePrice(order);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("has no products")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }

    [Test]
    public void CalculatePrice_OrderWithValidProducts_CalculatesCorrectPrices()
    {
        var order = new Order
        {
            OrderId = "1",
            Products = new List<OrderProduct>
            {
                new OrderProduct { ProductId = "1", Quantity = 2 },
                new OrderProduct { ProductId = "2", Quantity = 1 }
            }
        };

        _priceCalculator.CalculatePrice(order);

        Assert.That(32.0m, Is.EqualTo(order.GrossPrice));
        Assert.That(4.8m, Is.EqualTo(order.VATAmount));
        Assert.That(36.8m, Is.EqualTo(order.TotalPrice));
    }

    [Test]
    public void CalculatePrice_OrderWithInvalidProduct_LogsWarning()
    {
        var order = new Order
        {
            OrderId = "1",
            Products = new List<OrderProduct>
            {
                new OrderProduct { ProductId = "99", Quantity = 1 }
            }
        };

        _priceCalculator.CalculatePrice(order);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not found in products list")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }
}
