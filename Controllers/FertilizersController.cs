using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLandAPI.Data;
using SmartLandAPI.Models;
using SmartLandAPI.Services;
using System.Security.Claims;

namespace SmartLandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FertilizersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public FertilizersController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        

        // GET: api/Fertilizers
        [HttpGet]
        [Authorize(Policy = "RegisteredUsersOnly")]
        public async Task<ActionResult<IEnumerable<Fertilizer>>> GetFertilizers(
            [FromQuery] string? type = null)
        {
            var query = _context.Fertilizers.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(f => f.Type == type);
            }

           

            return await query.ToListAsync();
        }

        // GET: api/Fertilizers/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Fertilizer>> GetFertilizer(int id)
        {
            
            var fertilizer = await _context.Fertilizers.FindAsync(id);

            if (fertilizer == null)
            {
                return NotFound();
            }

            

            return fertilizer;
        }

        

        
    }

    public class FertilizerCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? NitrogenContent { get; set; }
        public string? PhosphorusContent { get; set; }
        public string? PotassiumContent { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
    }
}