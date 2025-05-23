using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using events.Models;
using System.Security.Claims;

namespace events.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(UserManager<IdentityUser> userManager) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]             [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        if (!ModelState.IsValid) 
        { 
            return BadRequest(ModelState);
        }
        
        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await userManager.CreateAsync(user, model.Password);
        
        if (result.Succeeded)
        {
            return Ok(new { message = "User registered successfully" });	
        }
        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }
    
    [HttpDelete("delete/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)] [ProducesResponseType(StatusCodes.Status404NotFound)] [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return Ok(new { message = "User deleted successfully" });
        }
        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        return Ok();
    }

}
