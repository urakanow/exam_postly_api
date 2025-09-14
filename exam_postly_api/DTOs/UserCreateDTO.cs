using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace exam_postly_api.DTOs
{
    public class UserCreateDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
