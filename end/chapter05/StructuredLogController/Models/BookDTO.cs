using System.ComponentModel.DataAnnotations;

namespace books.Models;

public class BookDTO
{
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime PublicationDate { get; set; }

    [Required]
    [StringLength(13, MinimumLength = 10)]
    public string ISBN { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Genre { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Summary { get; set; } = string.Empty;
}
