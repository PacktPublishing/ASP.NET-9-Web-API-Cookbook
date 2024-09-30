namespace books.GraphQL
{
    public class BookAlreadyExistsError : IUserError
    {
        public string Message { get; }
        public string Code => "BOOK_ALREADY_EXISTS";

        public BookAlreadyExistsError(string isbn)
        {
            Message = $"A book with ISBN '{isbn}' already exists.";
        }
    }
}
