using Books.Models;
namespace Books.GraphQL;

public class Subscription
{
    [Topic]
    [Subscribe]
    public BookDTO OnNewBookAdded([EventMessage] BookDTO book) => book;

} 
