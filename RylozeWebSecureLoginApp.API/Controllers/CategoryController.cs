using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RylozeWebSecureLoginApp.API.Data;

namespace RylozeWebSecureLoginApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
    }
}
