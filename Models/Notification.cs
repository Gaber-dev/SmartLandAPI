﻿using System.ComponentModel.DataAnnotations;

namespace SmartLandAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string ImageUrl { get; set; } 
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
 
