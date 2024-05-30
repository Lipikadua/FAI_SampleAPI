using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using Xunit;

namespace SampleAPI.Tests.Repositories
{
    public class OrderRepositoryTests : IDisposable
    {
        private readonly SampleApiDbContext _context;
        private readonly OrderRepository _repository;

        public OrderRepositoryTests()
        {
            _context = MockSampleApiDbContextFactory.GenerateMockContext();
            _repository = new OrderRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetRecentOrdersAsync_ShouldReturnRecentOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), Name = "Order1", Description = "Description1", EntryDate = DateTime.Now },
                new Order { Id = Guid.NewGuid(), Name = "Order2", Description = "Description2", EntryDate = DateTime.Now.AddDays(-1) },
                new Order { Id = Guid.NewGuid(), Name = "Order3", Description = "Description3", EntryDate = DateTime.Now }
            };

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRecentOrdersAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Any(o => o.Name == "Order2").Should().BeFalse();
            result.Any(o => o.Name == "Order1").Should().BeTrue();
            result.Any(o => o.Name == "Order3").Should().BeTrue();
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldAddOrder()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Name = "New Order",
                Description = "New Description",
                IsInvoiced = true
            };

            // Act
            var createdOrder = await _repository.CreateOrderAsync(order);

            // Assert
            var dbOrder = await _context.Orders.FindAsync(createdOrder.Id);
            dbOrder.Should().NotBeNull();
            dbOrder.Name.Should().Be(order.Name);
            dbOrder.Description.Should().Be(order.Description);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), Name = "Order1", Description = "Description1", IsDeleted = false },
                new Order { Id = Guid.NewGuid(), Name = "Order2", Description = "Description2", IsDeleted = true },
                new Order { Id = Guid.NewGuid(), Name = "Order3", Description = "Description3", IsDeleted = false }
            };

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllOrdersAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Any(o => o.Name == "Order2").Should().BeFalse();
            result.Any(o => o.Name == "Order1").Should().BeTrue();
            result.Any(o => o.Name == "Order3").Should().BeTrue();
        }

        [Fact]
        public async Task GetOrdersWithinDateRange_ShouldReturnOrdersInDateRange()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), Name = "Order1", Description = "Description1", EntryDate = DateTime.Now.AddDays(-5), IsDeleted = false },
                new Order { Id = Guid.NewGuid(), Name = "Order2", Description = "Description2", EntryDate = DateTime.Now.AddDays(-3), IsDeleted = false },
                new Order { Id = Guid.NewGuid(), Name = "Order3", Description = "Description3", EntryDate = DateTime.Now.AddDays(-1), IsDeleted = false },
                new Order { Id = Guid.NewGuid(), Name = "Order4", Description = "Description4", EntryDate = DateTime.Now, IsDeleted = true }
            };

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            var startDate = DateTime.Now.AddDays(-4);
            var endDate = DateTime.Now;

            // Act
            var result = await _repository.GetOrdersWithinDateRange(startDate, endDate);

            // Assert
            result.Should().HaveCount(3);
            result.Any(o => o.Name == "Order1").Should().BeFalse();
            result.Any(o => o.Name == "Order2").Should().BeTrue();
            result.Any(o => o.Name == "Order3").Should().BeTrue();
            result.Any(o => o.Name == "Order4").Should().BeFalse();
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnCorrectOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId, Name = "Order1", Description = "Description1", IsDeleted = false };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetOrderByIdAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(orderId);
            result.Name.Should().Be(order.Name);
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldUpdateOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId, Name = "Order1", Description = "Description1", IsDeleted = false };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            order.Name = "Updated Order";
            order.Description = "Updated Description";

            // Act
            var updatedOrder = await _repository.UpdateOrderAsync(order);

            // Assert
            var dbOrder = await _context.Orders.FindAsync(updatedOrder.Id);
            dbOrder.Should().NotBeNull();
            dbOrder.Name.Should().Be("Updated Order");
            dbOrder.Description.Should().Be("Updated Description");
        }
    }
}
