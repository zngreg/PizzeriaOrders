using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using PizzeriaOrders.Models;
using PizzeriaOrders.Services;
using System;
using System.Collections.Generic;

namespace PizzeriaOrders.Services.Unit.Tests
{
    [TestFixture]
    public class OrderValidatorTests
    {
        private Mock<ILogger<OrderValidator>> _loggerMock;
        private List<Product> _products;
        private OrderValidator _orderValidator;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<OrderValidator>>();
            _products = new List<Product>
            {
                new Product { ProductId = "prod1" },
                new Product { ProductId = "prod2" }
            };
            _orderValidator = new OrderValidator(_products, _loggerMock.Object);
        }

        [Test]
        public void IsValid_OrderIsNull_ReturnsInvalidResult()
        {
            var result = _orderValidator.IsValid(null);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Order is null."));
        }

        [Test]
        public void IsValid_DuplicateOrderId_ReturnsInvalidResult()
        {
            var order = new Order { OrderId = "order1", Products = new List<OrderProduct> { new OrderProduct { ProductId = "prod1", Quantity = 1 } }, CreatedAt = DateTime.Now, DeliverAt = DateTime.Now.AddHours(1), CustomerAddress = "Address" };

            _orderValidator.IsValid(order);
            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Duplicate OrderId found: order1"));
        }

        [Test]
        public void IsValid_OrderIdIsNullOrEmpty_ReturnsInvalidResult()
        {
            var order = new Order { OrderId = "", Products = new List<OrderProduct> { new OrderProduct { ProductId = "prod1", Quantity = 1 } }, CreatedAt = DateTime.Now, DeliverAt = DateTime.Now.AddHours(1), CustomerAddress = "Address" };

            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Order ID is null or empty."));
        }

        [Test]
        public void IsValid_ProductsListIsNullOrEmpty_ReturnsInvalidResult()
        {
            var order = new Order { OrderId = "order1", Products = null, CreatedAt = DateTime.Now, DeliverAt = DateTime.Now.AddHours(1), CustomerAddress = "Address" };

            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Products list is null or empty."));
        }

        [Test]
        public void IsValid_InvalidProductIds_ReturnsInvalidResult()
        {
            var order = new Order { OrderId = "order1", Products = new List<OrderProduct> { new OrderProduct { ProductId = "invalid", Quantity = 1 } }, CreatedAt = DateTime.Now, DeliverAt = DateTime.Now.AddHours(1), CustomerAddress = "Address" };

            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo("One or more product IDs in the order are invalid."));
        }

        [Test]
        public void IsValid_InvalidProductQuantities_ReturnsInvalidResult()
        {
            var order = new Order { OrderId = "order1", Products = new List<OrderProduct> { new OrderProduct { ProductId = "prod1", Quantity = 0 } }, CreatedAt = DateTime.Now, DeliverAt = DateTime.Now.AddHours(1), CustomerAddress = "Address" };

            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo("One or more product quantities in the order are invalid."));
        }

        [Test]
        public void IsValid_InvalidDeliveryTime_ReturnsInvalidResult()
        {
            var order = new Order { OrderId = "order1", Products = new List<OrderProduct> { new OrderProduct { ProductId = "prod1", Quantity = 1 } }, CreatedAt = DateTime.Now, DeliverAt = DateTime.Now, CustomerAddress = "Address" };

            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Delivery time '{order.DeliverAt}' is invalid."));
        }

        [Test]
        public void IsValid_CreationTimeInFuture_ReturnsInvalidResult()
        {
            var order = new Order { OrderId = "order1", Products = new List<OrderProduct> { new OrderProduct { ProductId = "prod1", Quantity = 1 } }, CreatedAt = DateTime.Now.AddHours(1), DeliverAt = DateTime.Now.AddHours(2), CustomerAddress = "Address" };

            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Order creation time '{order.CreatedAt}' is in the future."));
        }

        [Test]
        public void IsValid_CustomerAddressIsNullOrEmpty_ReturnsInvalidResult()
        {
            var order = new Order { OrderId = "order1", Products = new List<OrderProduct> { new OrderProduct { ProductId = "prod1", Quantity = 1 } }, CreatedAt = DateTime.Now, DeliverAt = DateTime.Now.AddHours(1), CustomerAddress = "" };

            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Is.EqualTo("Customer address is null or empty."));
        }

        [Test]
        public void IsValid_ValidOrder_ReturnsValidResult()
        {
            var order = new Order { OrderId = "order1", Products = new List<OrderProduct> { new OrderProduct { ProductId = "prod1", Quantity = 1 } }, CreatedAt = DateTime.Now, DeliverAt = DateTime.Now.AddHours(1), CustomerAddress = "Address" };

            var result = _orderValidator.IsValid(order);

            Assert.That(result.IsValid, Is.True);
        }
    }
}