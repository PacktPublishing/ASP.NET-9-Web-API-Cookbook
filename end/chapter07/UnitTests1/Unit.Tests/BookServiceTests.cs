using Xunit;
using Books.Repositories;
using Books.Models;
using Books.Services;

namespace Tests.Services; 

public class BooksServiceTests
{
    [Fact]
    public async Task GetBookById_ReturnsBookDTO_WhenBookExists()
    {
    // Arrange
    int testBookId = 1;
    var bookFromRepository = new Book
    {
        Id = testBookId,
        Title = "Test Book",
        Author = "Test Author",
        PublicationDate = new DateTime(2020, 1, 1),
        ISBN = "1234567890123",
        Genre = "Test Genre",
        Summary = "Test Summary"
    }; 

    var expectedBookDto = new BookDTO
    {
        Id = testBookId,
        Title = "Test Book",
        Author = "Test Author",
        PublicationDate = new DateTime(2020, 1, 1),
        ISBN = "1234567890123",
        Genre = "Test Genre",
        Summary = "Test Summary"
    }; 

    var repository = new FakeBooksRepository(bookFromRepository);
    var service = new BooksService(repository);

    // Act 
    var result = await service.GetBookByIdAsync(testBookId);

    // Assert 
    Assert.NotNull(result);
    Assert.Equal(expectedBookDto.Id, result.Id);
    Assert.Equal(expectedBookDto.Title, result.Title);
    Assert.Equal(expectedBookDto.Author, result.Author);
    Assert.Equal(expectedBookDto.PublicationDate, result.PublicationDate);
    Assert.Equal(expectedBookDto.ISBN, result.ISBN);
    Assert.Equal(expectedBookDto.Genre, result.Genre);
    Assert.Equal(expectedBookDto.Summary, result.Summary);

    }
}
