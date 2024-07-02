using System.ComponentModel.DataAnnotations;

namespace events.Models;

public class RegisterDTO
{
   [Required]
   [EmailAddress]
   public required string Email { get; set; }

   [Required]
   [StringLength(100, MinimumLength = 6)]
   public required string Password { get; set; }
}
