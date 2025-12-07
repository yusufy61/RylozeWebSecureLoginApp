using System.ComponentModel.DataAnnotations;

namespace RylozeWebSecureLoginApp.API.DTOs
{
    public class LoginDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;
    }
}
