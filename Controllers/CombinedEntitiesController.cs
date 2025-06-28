using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLandAPI.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SmartLandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CombinedEntitiesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;

        public CombinedEntitiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CombinedEntities
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<CombinedEntitiesResponse>> GetCombinedEntities(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            
            pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

            
            var plants = await _context.Plants
                .OrderBy(p => p.Name)
                .Select(p => new CombinedEntityDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Type = "Plant"
                }).ToListAsync();

            
            var crops = await _context.Crops
                .OrderBy(c => c.Name)
                .Select(c => new CombinedEntityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Type = "Crop"
                }).ToListAsync();

            
            var fertilizers = await _context.Fertilizers
                .OrderBy(f => f.Name)
                .Select(f => new CombinedEntityDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    ImageUrl = f.ImageUrl,
                    Type = "Fertilizer"
                }).ToListAsync();

            
            var combinedItems = plants.Concat(crops).Concat(fertilizers).OrderBy(x => x.Name).ToList();

            
            var totalCount = combinedItems.Count;
            var items = combinedItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new CombinedEntitiesResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        
    }

    public class CombinedEntitiesResponse
    {
        public List<CombinedEntityDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class CombinedEntityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Type { get; set; } // "Plant" or "Crop"  or "Fertilizer"
    }

    public class CategoryCountDto
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string ImageUrl { get; set; }
    }
}