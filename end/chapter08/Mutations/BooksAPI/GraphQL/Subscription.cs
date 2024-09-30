using books.Models;

namespace books.GraphQL;

public class Subscription 
{
    [Topic]
    [Subscribe]
    public BookDTO OnNewBookAdded([EventMessage] BookDTO book) => book;

}
