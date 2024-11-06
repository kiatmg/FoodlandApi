using FoodLand.Data;
using FoodLand.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FoodLand.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        

        public CartController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] int foodId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.FoodId == foodId);

            if (cartItem == null)
            {
                cartItem = new Cart
                {
                    UserId = userId,
                    FoodId = foodId,
                    Quantity = 1
                };
                _context.Carts.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            await _context.SaveChangesAsync();

           
            var totalPrice = await CalculateTotalPrice(userId);

            return Ok(new { cartItem, totalPrice });
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveFromCart([FromBody] int foodId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.FoodId == foodId);

            if (cartItem == null)
            {
                return NotFound();
            }

            cartItem.Quantity--;

            if (cartItem.Quantity <= 0)
            {
                _context.Carts.Remove(cartItem);
            }

            await _context.SaveChangesAsync();

           
            var totalPrice = await CalculateTotalPrice(userId);

            return Ok(new { totalPrice });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Food)
                .ToListAsync();

            var result = cartItems.Select(c => new
            {
                c.FoodId,
                c.Quantity,
                c.Food.Id,
                c.Food.Name,
                c.Food.ImageUrl,
                c.Food.Description,
                c.Food.DiscountPercentage,
                c.Food.Price,
                PriceAfterDiscount = c.Food.DiscountPercentage.HasValue
                    ? c.Food.Price - (c.Food.Price * (decimal)c.Food.DiscountPercentage.Value / 100)
                    : c.Food.Price
            }).ToList();

            var totalPrice = result.Sum(item => item.PriceAfterDiscount * item.Quantity);

            return Ok(new
            {
                Items = result,
                TotalPrice = totalPrice
            });
        }

        private async Task<decimal> CalculateTotalPrice(string userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Food)
                .ToListAsync();

            return cartItems.Sum(c =>
                c.Food.DiscountPercentage.HasValue
                ? (c.Food.Price - (c.Food.Price * (decimal)c.Food.DiscountPercentage.Value / 100)) * c.Quantity
                : c.Food.Price * c.Quantity);
        }
    }
}