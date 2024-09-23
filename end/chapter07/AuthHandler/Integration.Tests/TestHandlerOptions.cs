using Microsoft.AspNetCore.Authentication;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string UserName { get; set; } = "testuser123";
    public List<string> Roles { get; set; } = new List<string>();
}
