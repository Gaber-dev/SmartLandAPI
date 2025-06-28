using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLandAPI.Data;
using SmartLandAPI.Models;
using System.Text.RegularExpressions;

namespace SmartLandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context) => _context = context;

        

        [HttpGet("filter")]
        [Authorize]
        public async Task<IActionResult> FilterSearch(
            [FromQuery] string? season = null,
            [FromQuery] string? soilType = null,
            [FromQuery] string? waterNeeds = null,
            [FromQuery] string? fertilizerCompatibility = null,
            [FromQuery] string? cropType = null)
        {
            try
            {
                var filteredCrops = await FilterCrops(season, soilType, waterNeeds, cropType, fertilizerCompatibility);

                return Ok(new
                {
                    Status = "Success",
                    Results = filteredCrops.Count,
                    Data = filteredCrops,
                    AppliedFilters = new
                    {
                        season,
                        soilType,
                        waterNeeds,
                        fertilizerCompatibility,
                        cropType
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
        }

        private async Task<List<CropResponse>> FilterCrops(
            string? season,
            string? soilType,
            string? waterNeeds,
            string? cropType,
            string? fertilizerCompatibility)
        {
            var query = _context.Crops.AsQueryable();

            if (!string.IsNullOrEmpty(season) && season != "All")
                query = query.Where(c => c.Season == season);

            if (!string.IsNullOrEmpty(soilType) && soilType != "All")
                query = query.Where(c => c.SoilType == soilType);

            if (!string.IsNullOrEmpty(cropType))
                query = query.Where(c => c.CropType == cropType);

            
            var crops = await query.ToListAsync();

           
            if (!string.IsNullOrEmpty(waterNeeds))
            {
                crops = crops.Where(c =>
                {
                    if (string.IsNullOrEmpty(c.WaterNeeds))
                        return false;

                    var normalizedWaterNeeds = NormalizeWaterNeeds(c.WaterNeeds);

                    return waterNeeds.Equals(normalizedWaterNeeds, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }

            
            if (!string.IsNullOrEmpty(fertilizerCompatibility))
            {
                crops = crops.Where(c => IsCompatibleWithFertilizer(c, fertilizerCompatibility)).ToList();
            }

            var compatibleFertilizers = await _context.Fertilizers.ToListAsync();

            return crops.Select(c => new CropResponse
            {
                Id = c.Id,
                Name = c.Name,
                ScientificName = c.ScientificName,
                Description = c.Description,
                Season = c.Season,
                SoilType = c.SoilType,
                WaterNeeds = c.WaterNeeds,
                CropType = c.CropType,
                ImageUrl = c.ImageUrl,
                TemperatureRange = c.TemperatureRange,
                CompatibleFertilizers = GetCompatibleFertilizers(c, compatibleFertilizers)
            }).ToList();
        }

        private string NormalizeWaterNeeds(string waterNeeds)
        {
            if (string.IsNullOrEmpty(waterNeeds)) return null;

            waterNeeds = waterNeeds.ToLower().Trim();

            
            if (waterNeeds.Contains("moderate")) return "Medium";
            if (waterNeeds.Contains("high")) return "High";
            if (waterNeeds.Contains("low")) return "Low";

           
            var matches = Regex.Matches(waterNeeds, @"\d+")
                           .Select(m => int.Parse(m.Value))
                           .ToList();

            if (matches.Count == 0) return null;

            int avg = matches.Count == 1
                ? matches[0]
                : (matches[0] + matches[^1]) / 2;

            return avg switch
            {
                <= 350 => "Low",
                > 350 and <= 600 => "Medium",
                > 600 => "High"
            };
        }

        private bool IsCompatibleWithFertilizer(Crop crop, string fertilizerType)
        {
            if (string.IsNullOrEmpty(fertilizerType)) return true;
            if (fertilizerType.Equals("Both", StringComparison.OrdinalIgnoreCase)) return true;

            return crop.CropType?.ToLower() switch
            {
                "fruit" => fertilizerType.Equals("Chemical", StringComparison.OrdinalIgnoreCase),
                "vegetables" => fertilizerType.Equals("Organic", StringComparison.OrdinalIgnoreCase),
                _ => true
            };
        }

        private List<FertilizerDto> GetCompatibleFertilizers(Crop crop, List<Fertilizer> allFertilizers)
        {
            var compatibleTypes = crop.CropType?.ToLower() switch
            {
                "fruit" => new List<string> { "Chemical" },
                "vegetables" => new List<string> { "Organic" },
                _ => new List<string> { "Chemical", "Organic" }
            };

            return allFertilizers
                .Where(f => compatibleTypes.Contains(f.Type))
                .Select(f => new FertilizerDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Type = f.Type,
                    ImageUrl = f.ImageUrl
                })
                .ToList();
        }
    }

    public class CropResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ScientificName { get; set; }
        public string Description { get; set; }
        public string Season { get; set; }
        public string SoilType { get; set; }
        public string WaterNeeds { get; set; }
        public string CropType { get; set; }
        public string ImageUrl { get; set; }
        public string TemperatureRange { get; set; }
        public List<FertilizerDto> CompatibleFertilizers { get; set; }
    }

    public class FertilizerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }
    }
}