using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RylozeWebSecureLoginApp.API.Entities;

namespace RylozeWebSecureLoginApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);

            return Ok(new
            {
                UserId = userId,
                Email = email,
                Roles = roles,
                message = "Profil bilgileriniz başarıyla alındı."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Sadece Admin rolüne sahip kullanıcılar bu veriye erişebilir." });
        }

        [Authorize(Roles = "Antrenor,Admin")]
        [HttpGet("antrenor-or-admin")]
        public IActionResult AntrenorOrAdmin()
        {
            return Ok(new { message = "Sadece Antrenor veya Admin rolüne sahip kullanıcılar bu veriye erişebilir." });
        }

        [Authorize(Roles = "Ogrenci")]
        [HttpGet("student-only")]
        public IActionResult StudentOnly()
        {
            return Ok(new { message = "Öğrenci alanına hoş geldiniz!" });
        }


    }
}
