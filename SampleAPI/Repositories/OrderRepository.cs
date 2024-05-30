using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleAPI.Entities;
using SampleAPI.Requests;


namespace SampleAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SampleApiDbContext _context;
        public OrderRepository(SampleApiDbContext context)
        {
            _context = context;
        }
        public async Task<List<Order>> GetRecentOrdersAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted && o.EntryDate > DateTime.Now.AddDays(-1))
                .OrderByDescending(o => o.EntryDate)
                .ToListAsync();
        }
        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.EntryDate = DateTime.Now;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .ToListAsync();
        }
        public async Task<List<Order>> GetOrdersWithinDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.EntryDate >= startDate && o.EntryDate <= endDate && !o.IsDeleted)
                .OrderByDescending(o => o.EntryDate)
                .ToListAsync();
        }
        public async Task<Order> GetOrderByIdAsync(Guid id)
        {
            return await _context.Orders.FindAsync(id);
        }
        public async Task<Order> UpdateOrderAsync(Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return order;
        }
    }
}
