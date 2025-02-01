using Books.Repositories;
using Books.Services;
using NSubstitute;

namespace Tests.Services;

public abstract class BooksServiceTestsBase
{
    protected IBooksRepository? Repository { get; }
    protected BooksService? Service { get; }

    protected BooksServiceTestsBase()
    {
        Repository = Substitute.For<IBooksRepository>();
        Service = new BooksService(Repository);
    }
} 

