using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using SignalRServer.Models;
using SignalRServer.Hubs;

namespace SignalRServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        IHubContext<MessagingHub, IMessagingClient> hubContext) : ControllerBase
{

    [HttpGet("test")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public IActionResult Test()
    {
        return Ok("AuthController is working!");
    }

    [Authorize]
    [HttpGet("testAuth")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult AuthTest()
    {
        return Ok("AuthController authorize is working!");
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(IEnumerable<IdentityError>))]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = new IdentityUser { UserName = model.Username, Email = model.Username };
        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new { message = "User registered successfully" });
        }
        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var user = await userManager.FindByNameAsync(model.Username);
        if (user is not null && await userManager.CheckPasswordAsync(user, model.Password))
        {
            var userRoles = await userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user.UserName, userRoles, model.Locale);
            
            await hubContext.AnnounceUserLogin(model.Username);
            return Ok(new { token, user.UserName, Roles = userRoles });
        }
        return Unauthorized("Invalid username or password");
    }

    private string GenerateJwtToken(string userName, IList<string> roles, string locale)
    {
        var secret = configuration["Jwt:Key"] ?? 
            throw new ApplicationException("JWT Key configuration is not set");
        var issuer = configuration["Jwt:Issuer"] ?? 
            throw new ApplicationException("JWT Issuer configuration is not set");
        var audience = configuration["Jwt:Audience"] ?? 
            throw new ApplicationException("JWT Audience configuration is not set");

        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            throw new ApplicationException("JWT configuration is not set properly");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.NameIdentifier, userName),
            new Claim("locale", locale),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            NotBefore = DateTime.UtcNow,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
