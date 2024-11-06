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
    public class FoodController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public FoodController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        [HttpGet("category")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategoriesWithFoods()
        {
            var categories = await _context.Categories.Include(c => c.Foods).ToListAsync();

            if (!categories.Any())
            {
                return NotFound();
            }

            foreach (var category in categories)
            {
                foreach (var food in category.Foods)
                {
                    if (food.DiscountPercentage.HasValue)
                    {
                        food.PriceAfterDiscount = food.Price - (food.Price * (decimal)food.DiscountPercentage.Value / 100);
                    }
                }
            }

            return categories;
        }
    }
}
