using SampleAPI.Entities;
using SampleAPI.Requests;

namespace SampleAPI.Repositories
{
    public interface IOrderRepository
    {
        // TODO: Create repository methods.

        // Suggestions for repo methods:
        // public GetRecentOrders();
        // public AddNewOrder();

        Task<List<Order>> GetRecentOrdersAsync();
        Task<Order> CreateOrderAsync(Order order);
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<Order>> GetOrdersWithinDateRange(DateTime startDate, DateTime endDate);
        Task<Order> GetOrderByIdAsync(Guid id);
        Task<Order> UpdateOrderAsync(Order order);
    }
}
