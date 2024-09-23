using System.Net;
using System.Net.Http.Json;
using books.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests.Integration;

public class BooksControllerIntegrationTests : IClassFixture<CustomIntegrationTestsFixture>
{
    private readonly HttpClient _client;

    public BooksControllerIntegrationTests(CustomIntegrationTestsFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetBookById_ReturnsCorrectBook()
    {
        // Arrange
        var expectedBookId = 1; // Assuming the first seeded book has ID 1

        // Act
        var response = await _client.GetAsync($"/api/Books/{expectedBookId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var book = await response.Content.ReadFromJsonAsync<BookDTO>();
        book.Should().NotBeNull();
        book!.Id.Should().Be(expectedBookId);
        book.Title.Should().Be("Test Book 1");
        book.Author.Should().Be("Author 1");
        book.ISBN.Should().Be("1234567890");
    }

    [Fact]
    public async Task GetBookById_ReturnsNotFound_ForNonExistentBook()
    {
        // Arrange
        var nonExistentBookId = 9999;

        // Act
        var response = await _client.GetAsync($"/api/Books/{nonExistentBookId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
