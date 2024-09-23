using books.Models;
using books.Services;
using AutoFixture.Xunit2;
using AutoFixture;
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

    [Fact]
    public async Task GetBooksAsync_ReturnsPagedResult_WhenNoPreviousOrNextPage()
    {
        // Arrange
        int pageSize = 2;
        int lastId = 0;

        var booksFromRepository = Fixture.CreateMany<Book>(pageSize).ToList();

        Repository.GetBooksAsync(pageSize + 1, lastId).Returns(booksFromRepository);

       
        var expectedItems = booksFromRepository.Select(b => new BookDTO
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            PublicationDate = b.PublicationDate,
            ISBN = b.ISBN,
            Genre = b.Genre,
            Summary = b.Summary
        }).ToList();

        // Act
        var result = await Service.GetBooksAsync(pageSize, lastId, UrlHelper);

        // Assert
        result.Items.Should().BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
        result.PreviousPageUrl.Should().BeNull();
        result.NextPageUrl.Should().BeNull();
        result.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public async Task GetBooksAsync_ShouldReturnCorrectPagedResult()
    {
        // Arrange
        int pageSize = 2;
        int lastId = 0;

        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1" },
            new Book { Id = 2, Title = "Book 2" },
            new Book { Id = 3, Title = "Book 3" },
        };

        Repository.GetBooksAsync(pageSize + 1, lastId).Returns(books);

        // Act
        var result = await Service.GetBooksAsync(pageSize, lastId, UrlHelper);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(books.Take(2).Select(b => new BookDTO
        {
            Id = b.Id,
            Title = b.Title,
        }), options => options.ExcludingMissingMembers());

        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
        result.PreviousPageUrl.Should().BeNull();
        result.NextPageUrl.Should().NotBeNull("NextPageUrl should not be null when HasNextPage is true");
        if (result.NextPageUrl != null)
        {
            result.NextPageUrl.Should().Contain("action=GetBooks", "The action should be GetBooks");
            result.NextPageUrl.Should().Contain($"pageSize={pageSize}", "The pageSize should be included");
            result.NextPageUrl.Should().Contain($"lastId={books[1].Id}", "The lastId should be the ID of the last item on the current page");
        }
        result.PageSize.Should().Be(pageSize);
    }
}
