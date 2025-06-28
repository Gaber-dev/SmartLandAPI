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
    public class CropsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public CropsController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }



        // GET: api/Crops
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Crop>>> GetCrops(
            [FromQuery] string? season = null,
            [FromQuery] string? soilType = null,
            [FromQuery] string? waterNeeds = null)
        {
            var query = _context.Crops.AsQueryable();

            if (!string.IsNullOrEmpty(season))
            {
                query = query.Where(c => c.Season == season);
            }

            if (!string.IsNullOrEmpty(soilType))
            {
                query = query.Where(c => c.SoilType == soilType);
            }

            if (!string.IsNullOrEmpty(waterNeeds))
            {
                query = query.Where(c => c.WaterNeeds == waterNeeds);
            }

            return await query.ToListAsync();
        }

        // GET: api/Crops/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "RegisteredUsersOnly")]
        public async Task<ActionResult<Crop>> GetCrop(int id)
        {

            var crop = await _context.Crops.FindAsync(id);

            if (crop == null)
            {
                return NotFound();
            }
            return crop;
        }
    }

    public class CropCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? ScientificName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? SoilType { get; set; }
        public string? TemperatureRange { get; set; }
        public string? WaterNeeds { get; set; }
        public string? Season { get; set; }
        public string? ImageUrl { get; set; }
        public string CropType { get; set; } = string.Empty;
    }

    
    public class CropUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? ScientificName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? SoilType { get; set; }
        public string? TemperatureRange { get; set; }
        public string? WaterNeeds { get; set; }
        public string? Season { get; set; }
        public string? ImageUrl { get; set; }
        
        public string CropType { get; set; } = string.Empty;
    }
}