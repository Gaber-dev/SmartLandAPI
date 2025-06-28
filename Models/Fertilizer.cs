using System.ComponentModel.DataAnnotations;

namespace SmartLandAPI.Models
{
    public class Fertilizer
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? Type { get; set; } // Organic, Chemical
        public string? NitrogenContent { get; set; }
        public string? PhosphorusContent { get; set; }
        public string? PotassiumContent { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int? UserId { get; set; }

    }
}
