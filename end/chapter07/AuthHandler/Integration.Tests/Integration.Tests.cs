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
using books.Data;
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
        response.IsSuccessStatusCode.Should().BeTrue("the request should be successful");

        var content = await response.Content.ReadAsStringAsync();

    }

    [Fact]
    public async Task Auth_ShouldGetPastAuthEndpointJWT()
    {

        var client = _factory.CreateClientWithJwtAuth("testuser123", new[] { "Admin", "User" });

        // Act
        var response = await client.GetAsync("/api/Auth/testAuth");
        response.IsSuccessStatusCode.Should().BeTrue("the request should be successful");

        var content = await response.Content.ReadAsStringAsync();

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

        var content = await response.Content.ReadAsStringAsync();
        TestLogger.Log("Claims content returned:");
        TestLogger.Log(content);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue("the request should be successful");

        using (JsonDocument document = JsonDocument.Parse(content))
        {
            var root = document.RootElement;

            root.GetProperty("isAuthenticated").GetBoolean().Should().BeTrue("the user should be authenticated");
            root.GetProperty("authenticationType").GetString().Should().Be("Test", "the authentication type should match the test scheme");
            root.GetProperty("name").GetString().Should().Be("testuser123", "the name should match the test user");

            var claims = root.GetProperty("claims");
            claims.GetArrayLength().Should().Be(2, "there should be two claims");

            var claimValues = claims.EnumerateArray()
                                    .Select(c => c.GetProperty("value").GetString())
                                    .ToList();

            claimValues.Should().AllBe("testuser123", "all claim values should be 'testuser123'");
        }

        TestLogger.Log("All assertions passed successfully");
    }


    [Fact]
    public async Task Claims_ShouldReturnNameAndNameIdentifier()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/Books/claims");
        var content = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue();
        TestLogger.Log("Claims content returned...");
        TestLogger.Log(content);
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

        var content = await response.Content.ReadAsStringAsync();
        TestLogger.Log($"Response content: {content}");

        // Log response headers
        TestLogger.Log("Response headers:");
        foreach (var header in response.Headers)
        {
            TestLogger.Log($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        // Assert
        TestLogger.Log("Starting assertions");
        response.IsSuccessStatusCode.Should().BeTrue($"Expected successful status code, but got {response.StatusCode}");
        content.Should().Contain("#", $"Expected content to contain '#', but got: {content}");
        content.Should().NotBe("Hello ", $"Expected content to not be exactly 'Hello ', but got: {content}");
        content.Should().MatchRegex(@"Hello #\w+", $"Expected content to match 'Hello #<word>', but got: {content}");
        content.Should().Be($"Hello #testuser123");
        TestLogger.Log("Assertions completed");
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

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response content: {content}");

        // Log response headers
        Console.WriteLine("Response headers:");
        foreach (var header in response.Headers)
        {
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue($"Expected successful status code, but got {response.StatusCode}");
        content.Should().Contain("Hello", $"Expected content to contain 'Hello', but got: {content}");
    }

    [Fact]
    public async Task Claims_WithJWTShouldReturnNameAndNameIdentifier()
    {
        var client = _factory.CreateClientWithJwtAuth("testuser123", new[] { "Admin", "User" });

        var response = await client.GetAsync("/api/Books/claims");
        var content = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.Should().BeTrue();
        TestLogger.Log("Claims content returned...");
        TestLogger.Log(content);
    }
}
