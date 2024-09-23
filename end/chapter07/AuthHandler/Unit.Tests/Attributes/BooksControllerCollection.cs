using Xunit;

namespace Tests.Controllers;

[CollectionDefinition("BooksController Tests")]
public class BooksControllerCollection : ICollectionFixture<BooksControllerFixture>
{
}
