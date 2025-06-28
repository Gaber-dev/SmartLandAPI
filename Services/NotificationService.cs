using Microsoft.EntityFrameworkCore;
using SmartLandAPI.Data;
using SmartLandAPI.Models;

namespace SmartLandAPI.Services
{
    public class NotificationService : INotificationService
    {
        private AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, string imageUrl )
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                ImageUrl = imageUrl, // Add this property to your Notification model if not exists
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
