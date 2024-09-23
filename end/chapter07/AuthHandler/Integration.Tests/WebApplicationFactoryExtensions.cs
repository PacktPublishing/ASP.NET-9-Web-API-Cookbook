using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.TestHost;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Text;


public static class WebApplicationFactoryExtensions
{
    public static HttpClient CreateClientWithTestAuth<TProgram>(this WebApplicationFactory<TProgram> factory) 
        where TProgram : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(TimeProvider.System);
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, 
                    options => { });
            });
        })
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public static HttpClient CreateAuthenticatedClient<TProgram>(this WebApplicationFactory<TProgram> factory) 
        where TProgram : class
    {
        var client = factory.CreateClientWithTestAuth();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        return client;
    }


    public static HttpClient CreateClientWithJwtAuth<TProgram>(this WebApplicationFactory<TProgram> factory, string userName, IList<string> roles)
        where TProgram : class
    {
        var client = factory.CreateClient();
        var token = GenerateJwtToken(factory, userName, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        TestLogger.Log($"JWT Authorization header set for user: {userName}");
        return client;
    }

    private static string GenerateJwtToken<TProgram>(WebApplicationFactory<TProgram> factory, string userName, IList<string> roles)
        where TProgram : class
    {
        var configuration = factory.Services.GetRequiredService<IConfiguration>();
        var secret = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            throw new ApplicationException("JWT configuration is not set properly in the test environment");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.NameIdentifier, userName),
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
