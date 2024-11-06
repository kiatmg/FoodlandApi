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
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AddressController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return Ok(addresses);
        }

        [HttpPost]
        public async Task<ActionResult<Address>> PostAddress([FromBody] AddressViewModel addressModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
            var address = new Address
            {
                StreetAddress = addressModel.StreetAddress,
                UserId = userId
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAddresses), new { id = address.Id }, address);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAddress([FromBody] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var address = await _context.Addresses
                .Where(a => a.Id == id && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
