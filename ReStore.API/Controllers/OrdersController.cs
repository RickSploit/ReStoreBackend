using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.API.DTOs;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using System.Security.Claims;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Get current user's orders
        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Appliance)
                .Include(o => o.Buyer)
                .Where(o => o.BuyerId == userId.Value)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                PlatformCommission = o.PlatformCommission,
                Status = o.Status.ToString(),
                BuyerId = o.BuyerId,
                BuyerName = o.Buyer?.Name ?? string.Empty,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ApplianceId = oi.ApplianceId,
                    ApplianceTitle = oi.Appliance?.Title ?? string.Empty,
                    Price = oi.Price
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }

        // GET: Get specific order by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Appliance)
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Order not found." });

            // Allow access if user owns the order or is admin
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && order.BuyerId != userId.Value)
                return Forbid();

            var orderDto = new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PlatformCommission = order.PlatformCommission,
                Status = order.Status.ToString(),
                BuyerId = order.BuyerId,
                BuyerName = order.Buyer?.Name ?? string.Empty,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ApplianceId = oi.ApplianceId,
                    ApplianceTitle = oi.Appliance?.Title ?? string.Empty,
                    Price = oi.Price
                }).ToList()
            };

            return Ok(orderDto);
        }

        // POST: Create a new order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { message = "Order must have at least one item." });

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var appliance = await _context.Appliances.FindAsync(item.ApplianceId);
                if (appliance == null)
                    return NotFound(new { message = $"Appliance with ID {item.ApplianceId} not found." });

                if (appliance.Status != ApplianceStatus.Available)
                    return BadRequest(new { message = $"Appliance '{appliance.Title}' is not available." });

                totalAmount += item.Price > 0 ? item.Price : appliance.Price;

                orderItems.Add(new OrderItem
                {
                    ApplianceId = item.ApplianceId,
                    Price = item.Price > 0 ? item.Price : appliance.Price
                });

                // Mark appliance as reserved
                appliance.Status = ApplianceStatus.Reserved;
            }

            // Calculate 10% platform commission
            var platformCommission = totalAmount * 0.10m;

            var order = new Order
            {
                BuyerId = userId.Value,
                TotalAmount = totalAmount,
                PlatformCommission = platformCommission,
                Status = OrderStatus.Pending,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order created successfully!", orderId = order.Id });
        }

        // PUT: Update order status (Admin only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found." });

            if (Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
            {
                order.Status = newStatus;
            }
            else
            {
                return BadRequest(new { message = "Invalid status. Valid values: Pending, Shipped, Delivered, Cancelled" });
            }

            // If order is cancelled, release the appliances
            if (newStatus == OrderStatus.Cancelled)
            {
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == order.Id)
                    .ToListAsync();

                foreach (var item in orderItems)
                {
                    var appliance = await _context.Appliances.FindAsync(item.ApplianceId);
                    if (appliance != null)
                    {
                        appliance.Status = ApplianceStatus.Available;
                    }
                }
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully!" });
        }

        // GET: Admin get all orders
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Appliance)
                .Include(o => o.Buyer)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                PlatformCommission = o.PlatformCommission,
                Status = o.Status.ToString(),
                BuyerId = o.BuyerId,
                BuyerName = o.Buyer?.Name ?? string.Empty,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ApplianceId = oi.ApplianceId,
                    ApplianceTitle = oi.Appliance?.Title ?? string.Empty,
                    Price = oi.Price
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }

        // Helper method to get current user ID
        private int? GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
