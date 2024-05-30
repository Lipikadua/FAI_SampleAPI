using Microsoft.AspNetCore.Mvc;
using SampleAPI.Entities;
using SampleAPI.Repositories;
using SampleAPI.Requests;
using SampleAPI.Utilities;

namespace SampleAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        // Add more dependencies as needed.
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpGet("")] // TODO: Change route, if needed.
        [ProducesResponseType(StatusCodes.Status200OK)] // TODO: Add all response types
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            try
            {
                var orders = await _orderRepository.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching orders.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// TODO: Add an endpoint to allow users to create an order using <see cref="CreateOrderRequest"/>.
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
                return BadRequest("Order request is null.");

            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 100)
                return BadRequest("Invalid order name. Must be non-empty and up to 100 characters.");

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 100)
                return BadRequest("Invalid order description. Must be non-empty and up to 100 characters.");

            try
            {
                var order = new Order
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsInvoiced = request.IsInvoiced,
                    EntryDate = DateTime.Now
                };

                var createdOrder = await _orderRepository.CreateOrderAsync(order);
                return CreatedAtAction(nameof(GetRecentOrders), new { id = createdOrder.Id }, createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the order.");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentOrders()
        {
            try
            {
                var recentOrders = await _orderRepository.GetRecentOrdersAsync();
                return Ok(recentOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching recent orders.");
                return StatusCode(500, "Internal server error");
            }
        }

        //bonus checkpoint
        [HttpGet("recent-business-days")]
        public async Task<IActionResult> GetOrdersWithinBusinessDays(int numberOfDays)
        {
            try
            {
                // Calculate the date range based on business days
                var startDate = DateTimeHelper.CalculateStartDate(numberOfDays);
                var endDate = DateTime.Today;

                // Retrieve orders within the calculated date range
                var orders = await _orderRepository.GetOrdersWithinDateRange(startDate, endDate);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving orders within business days.");
                return StatusCode(500, "Internal server error");
            }
        }

        // In OrdersController

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(); // Return 404 if the order does not exist
                }

                // Soft delete the order
                order.IsDeleted = true;
                await _orderRepository.UpdateOrderAsync(order);

                return NoContent(); // Return 204 for successful deletion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the order.");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
