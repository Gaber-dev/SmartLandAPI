using System.ComponentModel.DataAnnotations;

namespace SmartLandAPI.Models
{
    public class Crop
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? ScientificName { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? TemperatureRange { get; set; }
        public string? WaterNeeds { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string SoilType {  get; set; }

        public string Season {  get; set; }
        public string? CropType { get; set; } // e.g., "Vegetables", "Fruits", "Grains", "Legumes"
        public int? UserId { get; internal set; }

    }
}
