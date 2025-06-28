using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLandAPI.Data;
using SmartLandAPI.Services;
using System.Security.Claims;

namespace SmartLandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public HomeController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetHomeData()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            

            
            var plants = await _context.Plants
                .OrderBy(p => p.Name)
                .Take(2)
                .Select(p => new { p.Id, p.Name, p.Description, p.ImageUrl })
                .ToListAsync();

            var crops = await _context.Crops
                .OrderBy(c => c.Name)
                .Take(2)
                .Select(c => new { c.Id, c.Name, c.Description, c.ImageUrl })
                .ToListAsync();

            var fertilizers = await _context.Fertilizers
                .OrderBy(f => f.Name)
                .Take(2)
                .Select(f => new { f.Id, f.Name, f.Description, f.ImageUrl })
                .ToListAsync();

            return Ok(new
            {
                
                Plants = plants,
                Crops = crops,
                Fertilizers = fertilizers
            });
        }

    }
}


