using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using books.Repositories;
using NSubstitute;
using books.Models;
using books.Services;
using Microsoft.AspNetCore.Mvc;


namespace Tests.Services;

// BooksServiceTests.cs

public class BooksServiceTests : BooksServiceTestsBase
{
    [Theory]
    [InlineAutoData(1)]
    [InlineAutoData(2)]
    [InlineAutoData(3)]
    public async Task GetBookByIdTheory_ReturnsBookDTO_WhenBookExists(
        int testBookId, Book bookFromRepository)
    {
        // Arrange
    bookFromRepository.Id = testBookId;

    Repository.GetBookByIdAsync(testBookId).Returns(bookFromRepository);


    // Act
    var result = await Service.GetBookByIdAsync(testBookId);

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
}

public abstract class BooksServiceTestsBase
{
    protected Fixture Fixture { get; }
    protected IBooksRepository Repository { get; }
    protected IUrlHelper UrlHelper { get; }
    protected BooksService Service { get; }

    protected BooksServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize(new AutoNSubstituteCustomization());

        Repository = Substitute.For<IBooksRepository>();
        UrlHelper = Substitute.For<IUrlHelper>();
        Service = new BooksService(Repository);
    }
}

