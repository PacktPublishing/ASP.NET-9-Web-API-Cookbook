using books.Repositories;
using books.Models;
using books.Services;
using books.Controllers;
using AutoFixture;
using NSubstitute;

namespace Tests.Controllers;

public class BooksControllerFixture
{
    public BooksController BooksController { get; }
    public Fixture Fixture { get; }
    public IBooksService BooksService { get; }

    public BooksControllerFixture()
    {
        // Initialize AutoFixture
        Fixture = new Fixture();

        // Use AutoFixture to mock services (or use NSubstitute manually)
        BooksService = Substitute.For<IBooksService>();

        // Create an instance of BooksController, passing in the mocked service
        BooksController = new BooksController(BooksService);
    }
}

