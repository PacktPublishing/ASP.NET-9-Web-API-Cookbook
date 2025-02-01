using Xunit;
using AutoFixture.Xunit2;
using FluentAssertions.AspNetCore.Mvc;
using Books.Controllerss;
using Books.Services;
using Books.Models;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

[Collection("BooksController Tests")]
public class BooksControllerTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task GetBookById_ReturnsOk_WhenBookExists(
        int testBookId,
        BookDTO bookDto,
        IBooksService booksService)
    {
        // Arrange
        booksService.GetBookByIdAsync(testBookId).Returns(bookDto);
        var controller = new BooksController(booksService);

        // Act
        var result = await controller.GetBookById(testBookId);

        // Assert
        result.Should().BeOkObjectResult()
              .WithValue(bookDto)
              .WithStatusCode(StatusCodes.Status200OK);
    }

    
    [Theory]
    [AutoNSubstituteData]
    public async Task GetBookById_ReturnsNotFound_WhenBookDoesNotExist(
        int testBookId,
        IBooksService booksService)
    {
        // Arrange
        booksService.GetBookByIdAsync(testBookId).Returns((BookDTO)null!);
        var controller = new BooksController(booksService);

        // Act
        var result = await controller.GetBookById(testBookId);

        // Assert
        result.Should().BeNotFoundResult();
    }
}
