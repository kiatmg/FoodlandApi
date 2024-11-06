using FoodLand.Data;
using FoodLand.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodLand.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] int addressId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var address = await _context.Addresses.FindAsync(addressId);

            if (address == null || address.UserId != userId)
            {
                return BadRequest("Invalid address");
            }

            var cartItems = await _context.Carts
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Food)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return BadRequest("Cart is empty");
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                StreetAddress = address.StreetAddress,
                TotalPrice = 0
            };

            foreach (var item in cartItems)
            {
                var price = item.Food.DiscountPercentage.HasValue
                    ? item.Food.Price - (item.Food.Price * (decimal)item.Food.DiscountPercentage.Value / 100)
                    : item.Food.Price;

                var orderItem = new OrderItem
                {
                    FoodId = item.FoodId,
                    FoodName = item.Food.Name,
                    Price = price,
                    Quantity = item.Quantity,
                    Order = order
                };

                order.TotalPrice += price * item.Quantity;
                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            _context.Carts.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(order);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ToListAsync();

            return Ok(orders);
        }
    }
}
