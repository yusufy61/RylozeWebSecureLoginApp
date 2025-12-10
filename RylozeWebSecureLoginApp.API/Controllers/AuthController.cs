using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RylozeWebSecureLoginApp.API.DTOs;
using RylozeWebSecureLoginApp.API.Entities;
using RylozeWebSecureLoginApp.API.Interfaces;

namespace RylozeWebSecureLoginApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthController(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        /// <summary>
        /// Yeni kullanıcı kaydı (Admin, Antrenor, Ogrenci)
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            // Burada rolleri gösteriyoruz. Ve bu diziye göre kontrol sağlıyoruz.
            var validRoles = new[] { "Admin", "Antrenor", "Ogrenci" };
            if (!validRoles.Contains(registerDTO.Role))
            {
                return BadRequest("Geçersiz rol. Geçerli roller: Admin, Antrenor, Ogrenci.");
            }

            var user = new AppUser
            {
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                FullName = registerDTO.FullName,
                CreatedAt = DateTime.Now
            };

            // Yeni bir kullanıcı oluşturma isteği gönderiyoruz. eğerki başarılı olmazsa hata mesajı döndürüyoruz.
            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description)});
            }

            await _userManager.AddToRoleAsync(user, registerDTO.Role);

            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu", userId = user.Id });
        }


        /// <summary>
        /// Giriş yap ve JWT token al
        /// </summary>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            // Kullanıcıyı bul ve varlığını kontrol et
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if(user == null)
            {
                return Unauthorized("Geçersiz email veya şifre.");
            }

            // Kullanıcı var ama email ile şifre doğru mu kontrol et
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Geçersiz email veya şifre.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Refresh token ı veri tabanına kaydet
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
            await _userManager.UpdateAsync(user);

            var response = new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!) * 60,
                TokenType = "Bearer"
            };

            return Ok(response);
        }

        /// <summary>
        /// Refresh token ile yeni access token al
        /// </summary>
        /// <param name="refreshTokenDTO"></param>
        /// <returns></returns>
        [AllowAnonymous] // Refresh token endpoint'i authentication gerektirmez
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            // Öncelikle refresh token ı doğrula
            var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDTO.RefreshToken);
            if (principal == null)
            {
                return BadRequest(new {message = "Geçersiz token"});
            }

            // Kullanıcıyı bul
            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Geçersiz token" });
            }

            // Veritabanından kullanıcıyı al ve refresh token ı kontrol et
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null || user.RefreshToken != refreshTokenDTO.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest(new { message = "Geçersiz token veya token süresi dolmuş" });
            }

            // Yeni tokenları oluştur
            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Yeni refresh token ı veritabanına kaydet
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
            await _userManager.UpdateAsync(user);

            var response = new TokenResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!) * 60,
                TokenType = "Bearer"
            };

            return Ok(response);
        }


        /// <summary>
        /// Çıkış yap (Refresh token ı iptal et)
        /// </summary>
        /// <returns></returns>
        [Authorize] // Logout için authentication gerekli
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Kullanıcıyı bul
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new {message = "Kullanıcı Bulunamadı."});
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "Kullanıcı Bulunamadı." });
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            return Ok(new {message = "Başarıyla çıkış yapıldı." });
        }
    }
}
