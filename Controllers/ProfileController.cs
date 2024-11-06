using FoodLand.Data;
using FoodLand.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FoodLand.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        

        public ProfileController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrEmpty(model.Name))
            {
                var nameClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "Name");
                if (nameClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, nameClaim);
                    await _userManager.AddClaimAsync(user, new Claim("Name", model.Name));
                }
                else
                {
                    await _userManager.AddClaimAsync(user, new Claim("Name", model.Name));
                }
            }

            if (!string.IsNullOrEmpty(model.Username))
            {
                user.UserName = model.Username;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);
            }

            if (!string.IsNullOrEmpty(model.Password) && model.Password == model.ConfirmPassword)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

               
                await _signInManager.SignOutAsync();
                return Ok(new { message = "Password updated, please log in again." });
            }

            return Ok();
        }
    }
}