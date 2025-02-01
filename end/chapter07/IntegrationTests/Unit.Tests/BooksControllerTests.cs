using FluentAssertions.AspNetCore.Mvc;
using books.Controllers;
using books.Services;
using books.Models;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;

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
        IActionResult result = await controller.GetBookById(testBookId);

        // Assert
        result.Should()
            .BeOkObjectResult()
            .WithValueEquivalentTo<BookDTO>(bookDto);

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
