using System.ComponentModel.DataAnnotations;

namespace RylozeWebSecureLoginApp.API.DTOs
{
    public class RegisterDTO
    {

        [Required]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;


        [Required, MinLength(6)]
        public string Password { get; set; } = default!;

        [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        public string Role { get; set; } = "Ogrenci";
    }
}
