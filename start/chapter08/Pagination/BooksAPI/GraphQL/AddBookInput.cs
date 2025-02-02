using System.ComponentModel.DataAnnotations;

namespace Books.GraphQL;

public record AddBookInput(
    [Required][MaxLength(200)] string Title,
    [Required][MaxLength(100)] string Author,
    [Required] DateTime PublicationDate,
    [Required][StringLength(13, MinimumLength = 10)] string ISBN,
    [Required][MaxLength(50)] string Genre,
    [MaxLength(1000)] string? Summary);
