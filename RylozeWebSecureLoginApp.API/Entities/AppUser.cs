using Microsoft.AspNetCore.Identity;

namespace RylozeWebSecureLoginApp.API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string? FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
