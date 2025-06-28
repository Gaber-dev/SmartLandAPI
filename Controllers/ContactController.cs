using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartLandAPI.Models;
using SmartLandAPI.Services;

namespace SmartLandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public ContactController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendContactMessage([FromBody] ContactRequest request)
        {
            var subject = "New Contact Us Message";
            var body = $@"
            <h3>New Contact Message</h3>
            <p><strong>Name:</strong> {request.Name}</p>
            <p><strong>Email:</strong> {request.Email}</p>
            <p><strong>Phone:</strong> {request.Phone}</p>
            <p><strong>Message:</strong><br>{request.Message}</p>";

            await _emailService.SendEmailAsync("youremail@yourdomain.com", subject, body);

            return Ok(new { Message = "We appreciate your interest,Your message has been sent successfully,We will contact with you." });
        }
    }
}

