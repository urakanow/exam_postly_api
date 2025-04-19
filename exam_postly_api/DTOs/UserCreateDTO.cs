using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace exam_postly_api.DTOs
{
    public class UserCreateDTO
    {
        [Required, NotNull]
        public string Username { get; set; }

        [Required, NotNull]
        [EmailAddress]
        public string Email { get; set; }

        [Required, NotNull]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
