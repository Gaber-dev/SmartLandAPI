using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLandAPI.Data;
using SmartLandAPI.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartLandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Notifications
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

           
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid User");
            }

            
            var totalItems = await _context.Notifications
                .CountAsync(n => n.UserId == userId);

            if (totalItems == 0)
            {
                return Ok(new
                {
                    TotalItems = 0,
                    Page = page,
                    PageSize = pageSize,
                    Data = new List<Notification>()
                });
            }

            
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalItems = totalItems,
                Data = notifications
            });
        }

        
    }
}