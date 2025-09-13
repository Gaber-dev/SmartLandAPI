using System.ComponentModel.DataAnnotations;


namespace SmartLandAPI.Models
{
    public class Plant
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? ScientificName { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? LightRequirements { get; set; }
        public string? TemperatureRange { get; set; }
        public string? WaterNeeds { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }



        
        public int? UserId { get; set; }

        


    }
}

