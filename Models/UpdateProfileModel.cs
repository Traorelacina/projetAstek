using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class UpdateProfileModel
    {
        [Required]
        [StringLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
