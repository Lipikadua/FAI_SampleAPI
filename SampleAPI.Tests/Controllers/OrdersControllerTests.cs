using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SampleAPI.Controllers;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using SampleAPI.Requests;
using SampleAPI.Utilities;
using Xunit;

namespace SampleAPI.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly OrdersController _controller;
        private readonly Mock<IOrderRepository> _mockRepo;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;

        public OrdersControllerTests()
        {
            _mockRepo = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetOrders_ReturnsOkResult_WithListOfOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), Name = "Order1", Description = "Description1", EntryDate = DateTime.Now },
                new Order { Id = Guid.NewGuid(), Name = "Order2", Description = "Description2", EntryDate = DateTime.Now }
            };
            _mockRepo.Setup(repo => repo.GetAllOrdersAsync()).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetOrders();

            // Assert
            var actionResult = result.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.Value.Should().BeAssignableTo<List<Order>>().Which.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetOrders_InternalServerError_Returns500()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetAllOrdersAsync()).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetOrders();

            // Assert
            var actionResult = result.Result as ObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(500);
            actionResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedAtActionResult_WithOrder()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                Name = "New Order",
                Description = "New Description",
                IsInvoiced = true
            };
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsInvoiced = request.IsInvoiced,
                EntryDate = DateTime.Now
            };
            _mockRepo.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(order);

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var createdOrder = createdAtActionResult.Value.Should().BeAssignableTo<Order>().Subject;
            createdOrder.Name.Should().Be(request.Name);
            createdOrder.Description.Should().Be(request.Description);
        }

        [Fact]
        public async Task CreateOrder_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                // No name provided, which should trigger a validation error
                Description = "New Description",
                IsInvoiced = true
            };

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var errorMessage = badRequestResult.Value.Should().BeAssignableTo<string>().Subject;
            errorMessage.Should().Contain("Invalid order name");
        }

        [Fact]
        public async Task DeleteOrder_ExistingOrder_DeletesOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId, Name = "Test Order", Description = "Test Description", EntryDate = DateTime.Now };

            _mockRepo.Setup(repo => repo.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _controller.DeleteOrder(orderId);

            // Assert
            result.Should().BeOfType<NoContentResult>(); // Expecting 204 No Content for successful deletion
            order.IsDeleted.Should().BeTrue(); // Ensure the order is soft deleted
        }

        [Fact]
        public async Task DeleteOrder_NonExistentOrder_ReturnsNotFound()
        {
            // Arrange
            var nonExistentOrderId = Guid.NewGuid();

            _mockRepo.Setup(repo => repo.GetOrderByIdAsync(nonExistentOrderId)).ReturnsAsync((Order)null); // Return null to simulate non-existent order

            // Act
            var result = await _controller.DeleteOrder(nonExistentOrderId);

            // Assert
            result.Should().BeOfType<NotFoundResult>(); // Expecting 404 Not Found for non-existent order
        }

        [Fact]
        public async Task GetRecentOrders_ReturnsOkResult_WithListOfOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), Name = "Order1", Description = "Description1", EntryDate = DateTime.Now },
                new Order { Id = Guid.NewGuid(), Name = "Order2", Description = "Description2", EntryDate = DateTime.Now }
            };
            _mockRepo.Setup(repo => repo.GetRecentOrdersAsync()).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetRecentOrders();

            // Assert
            var actionResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrders = actionResult.Value.Should().BeAssignableTo<List<Order>>().Subject;
            returnedOrders.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetOrdersWithinBusinessDays_ReturnsOkResult_WithListOfOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), Name = "Order1", Description = "Description1", EntryDate = DateTime.Now.AddDays(-2) },
                new Order { Id = Guid.NewGuid(), Name = "Order2", Description = "Description2", EntryDate = DateTime.Now.AddDays(-3) }
            };
            _mockRepo.Setup(repo => repo.GetOrdersWithinDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetOrdersWithinBusinessDays(5);

            // Assert
            var actionResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrders = actionResult.Value.Should().BeAssignableTo<List<Order>>().Subject;
            returnedOrders.Count.Should().Be(2);
        }
    }
}