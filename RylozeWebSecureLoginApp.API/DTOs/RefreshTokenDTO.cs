using System.ComponentModel.DataAnnotations;

namespace RylozeWebSecureLoginApp.API.DTOs
{
    public class RefreshTokenDTO
    {
        [Required]
        public string RefreshToken { get; set; } = default!;
    }
}
