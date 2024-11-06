using FoodLand.Data;
using FoodLand.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodLand.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model.Password != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var user = new IdentityUser { UserName = model.Username };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized();

            var token = GenerateJwtToken(user);
            var favorites = await GetFavoritesForUser(user.Id);
            var cartItems = await GetCartItemsForUser(user.Id);
            var cartCount = cartItems.Sum(ci => ci.Quantity);

            var claims = await _userManager.GetClaimsAsync(user);
            var nameClaim = claims.FirstOrDefault(c => c.Type == "Name");
            var name = nameClaim?.Value;

            return Ok(new { token, user, favorites, cartItems, cartCount, name });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<List<Food>> GetFavoritesForUser(string userId)
        {
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

        private async Task<List<Cart>> GetCartItemsForUser(string userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Food)
                .ToListAsync();

            foreach (var item in cartItems)
            {
                if (item.Food.DiscountPercentage.HasValue)
                {
                    item.Food.PriceAfterDiscount = item.Food.Price - (item.Food.Price * (decimal)item.Food.DiscountPercentage.Value / 100);
                }
            }

            return cartItems;
        }
    }
}