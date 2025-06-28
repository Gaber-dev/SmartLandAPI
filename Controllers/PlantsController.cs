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
    public class PlantsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public PlantsController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

       

        // GET: api/Plants
        [HttpGet]
        [Authorize] 
        public async Task<ActionResult<IEnumerable<Plant>>> GetPlants(
            [FromQuery] string? lightRequirements = null,
            [FromQuery] string? waterNeeds = null)
        {
            var query = _context.Plants.AsQueryable();

            if (!string.IsNullOrEmpty(lightRequirements))
            {
                query = query.Where(p => p.LightRequirements == lightRequirements);
            }

            if (!string.IsNullOrEmpty(waterNeeds))
            {
                query = query.Where(p => p.WaterNeeds == waterNeeds);
            }
            return await query.ToListAsync();
        }

        // GET: api/Plants/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Plant>> GetPlant(int id)
        {
            
            var plant = await _context.Plants.FindAsync(id);

            if (plant == null)
            {
                return NotFound();
            }
            return plant;
        }

    }

    public class PlantCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? ScientificName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? LightRequirements { get; set; }
        public string? TemperatureRange { get; set; }
        public string? WaterNeeds { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
    }
}