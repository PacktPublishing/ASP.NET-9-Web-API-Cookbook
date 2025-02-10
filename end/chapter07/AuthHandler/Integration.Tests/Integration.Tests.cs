using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using books;
using FluentAssertions;
using Books.Data;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using System.Security.Claims;


public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthenticationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Auth_ShouldGetPastAuthEndpoint()
    {

        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/Auth/testAuth");

        response.Should().Be200Ok()
            .And.MatchInContent("AuthController authorize is working!");
    }

    [Fact]
    public async Task Auth_ShouldGetPastAuthEndpointJWT()
    {

        var client = _factory.CreateClientWithJwtAuth("testuser123", new[] { "Admin", "User" });

        // Act
        var response = await client.GetAsync("/api/Auth/testAuth");

        response.Should().Be200Ok()
            .And.MatchInContent("AuthController authorize is working!");
    }

    [Fact]
    public async Task Claims_ShouldReturnCorrectAuthenticationDetails()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        TestLogger.Log($"Authorization header set: {client.DefaultRequestHeaders.Authorization}");

        // Act
        TestLogger.Log("Sending request to /api/Books/claims");
        var response = await client.GetAsync("/api/Books/claims");


        TestLogger.Log($"Response received. Status code: {response.StatusCode}");
        response.Should().Be200Ok()
            .And.BeAs(new
            {
                isAuthenticated = true,
                authenticationType = "Test",
                name = "testuser123",
                claims = new[]
                {
                    new { type="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", value= "testuser123"},
                    new { type="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", value= "testuser123"},
                }
            });
    }


    [Fact]
    public async Task Claims_ShouldReturnNameAndNameIdentifier()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/Books/claims");

        response.Should().Be200Ok()
           .And.BeAs(new
           {
               claims = new[]
               {
                    new { type="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", value= "testuser123"},
                    new { type="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", value= "testuser123"},
               }
           });
    }

    [Fact]
    public async Task Hi_ShouldReturnNameIdentifier()
    {
        var client = _factory.CreateAuthenticatedClient();
        TestLogger.Log($"Authorization header set: {client.DefaultRequestHeaders.Authorization}");

        // Act
        TestLogger.Log("Sending request to /api/Books/hi");
        var response = await client.GetAsync("/api/Books/hi");
        TestLogger.Log($"Response received. Status code: {response.StatusCode}");

        response.Should().Be200Ok()
            .And.Satisfy(async response =>
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("#", $"Expected content to contain '#', but got: {content}");
                content.Should().NotBe("Hello ", $"Expected content to not be exactly 'Hello ', but got: {content}");
                content.Should().MatchRegex(@"Hello #\w+", $"Expected content to match 'Hello #<word>', but got: {content}");
                content.Should().Be($"Hello #testuser123");
            });
    }

    [Fact]
    public async Task Hi_EndpointIsOn()
    {
        // Arrange
        var client = _factory.CreateClient();
        Console.WriteLine("Client created");

        // Act
        Console.WriteLine("Sending request to /api/Books/hi..");
        var response = await client.GetAsync("/api/Books/hi");
        Console.WriteLine($"Response received. Status code: {response.StatusCode}");

        response.Should().Be200Ok().And.MatchInContent("*Hello*");
    }

    [Fact]
    public async Task Claims_WithJWTShouldReturnNameAndNameIdentifier()
    {
        var client = _factory.CreateClientWithJwtAuth("testuser123", new[] { "Admin", "User" });

        var response = await client.GetAsync("/api/Books/claims");

        response.Should().Be200Ok()
            .And.Satisfy(givenModelStructure: new
            {
                isAuthenticated = default(bool),
                authenticationType = default(string),
                name = default(string),
                claims = new[] { new { type = default(string), value = default(string) } }
            }, model =>
                {
                    model.Should().BeEquivalentTo(
                        new
                        {
                            isAuthenticated = true,
                            authenticationType = "AuthenticationTypes.Federation",
                            name = "testuser123"
                        });

                    model.claims.Should().NotBeNull().And.NotBeEmpty();
                    model.claims.Should().Contain(c => c.type == "iss" && c.value == "example-books.com");

                });
    }
}
