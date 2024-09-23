using books.Models;
using books.Services;
using AutoFixture.Xunit2;
using NSubstitute;
using FluentAssertions;

namespace Tests.Services; 

public class BooksServiceTests : BooksServiceTestsBase
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

    [Theory]
    [InlineAutoData(1)]
    [InlineAutoData(2)]
    [InlineAutoData(3)]
    public async Task GetBookByIdTheory_ReturnsBookDTO_WhenBookExists(int testBookId, Book bookFromRepository)
    {

    // Arrange
    bookFromRepository.Id = testBookId;

    Repository!.GetBookByIdAsync(testBookId).Returns(bookFromRepository);
    // Act 
    var result = await Service!.GetBookByIdAsync(testBookId);

    // Assert 
    result.Should().BeEquivalentTo(bookFromRepository, options => options.ExcludingMissingMembers());

    }
}
