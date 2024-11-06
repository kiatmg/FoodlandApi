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
    public class FavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
       
        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
           
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToFavorites([FromBody] int foodId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorite = new Favorite
            {
                UserId = userId,
                FoodId = foodId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            var food = await _context.Foods.FirstOrDefaultAsync(f => f.Id == foodId);
            if (food == null)
            {
                return NotFound("Food not found");
            }

            if (food.DiscountPercentage.HasValue)
            {
                food.PriceAfterDiscount = food.Price - (food.Price * (decimal)food.DiscountPercentage.Value / 100);
            }

            return Ok(food);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveFromFavorites([FromBody] int foodId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FoodId == foodId);

            if (favorite == null)
            {
                return NotFound();
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Food>>> GetFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favoriteFoods = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Food)
                .ToListAsync();

            foreach (var food in favoriteFoods)
            {
                if (food.DiscountPercentage.HasValue)
                {
                    food.PriceAfterDiscount = food.Price - (food.Price * (decimal)food.DiscountPercentage.Value / 100);
                }
            }

            return favoriteFoods;
        }
    }
}
