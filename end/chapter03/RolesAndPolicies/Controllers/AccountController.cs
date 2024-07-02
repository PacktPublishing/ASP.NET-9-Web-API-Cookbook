using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using events.Models;



namespace events.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;

        public AccountController(UserManager<IdentityUser> userManager, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;

            Console.WriteLine($"AccountController Constructor: JwtSettings Key length: {_jwtSettings.Key?.Length ?? 0}");
            Console.WriteLine($"AccountController Constructor: Issuer: {_jwtSettings.Issuer}");
            Console.WriteLine($"AccountController Constructor: Audience: {_jwtSettings.Audience}");

        }

        [HttpPost("register")]
		[ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
			if (!ModelState.IsValid) 
			{ 
				return BadRequest(ModelState);
			}
			
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
			
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
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User deleted successfully" });
            }
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await GenerateJwtToken(user);
                return Ok(token);
            }
            return Unauthorized(new { message = "Invalid login attempt" });
        }


        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
             if (string.IsNullOrEmpty(user.UserName))
            {
                throw new InvalidOperationException("User does not have a valid username");
            }

            Console.WriteLine($"GenerateJwtToken: JwtSettings Key length: {_jwtSettings.Key?.Length ?? 0}");
            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                throw new InvalidOperationException("JWT key is null or empty");
            }
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };


            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_jwtSettings.ExpirationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
