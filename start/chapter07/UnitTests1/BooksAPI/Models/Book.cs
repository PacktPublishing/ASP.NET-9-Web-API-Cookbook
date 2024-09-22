namespace books.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublicationDate { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
