using SmartLandAPI.Data;
using SmartLandAPI.Models;
using SmartLandAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net.Mail;
using Google.Apis.Auth;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel.DataAnnotations;

namespace SmartLandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IFileUploadService _fileUploadService;
        
        public AuthController(AppDbContext context, IAuthService authService, IEmailService emailService , IWebHostEnvironment webHostEnvironment , IFileUploadService fileUploadService )
        {
            _context = context;
            _authService = authService;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _fileUploadService = fileUploadService;
            
        }

        
        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = _authService.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully." });
        }

        
        
        [HttpPost("login")]
        [Authorize]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { Token = token });
        }
        
        
        
        
        
        [HttpPost("forgot-password")]
        [Authorize]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            
            var existingResets = await _context.PasswordResets
                .Where(pr => pr.UserId == user.Id && !pr.IsUsed)
                .ToListAsync();

            foreach (var reset in existingResets)
            {
                reset.IsUsed = true;
            }

            var resetCode = _authService.GenerateResetCode();

            var passwordReset = new PasswordReset
            {
                UserId = user.Id,
                Code = resetCode,
                Expiry = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            _context.PasswordResets.Add(passwordReset);
            await _context.SaveChangesAsync();

            var subject = "SmartLand - Password Reset Code";
            var body = $@"
            <h2>Password Reset Request</h2>
            <p>Your password reset code is: <strong>{resetCode}</strong></p>
            <p>This code will expire in 15 minutes.</p>
            ";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, body);
                return Ok(new { Email = user.Email, Message = "Reset code sent to your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }
        }

        // Verify Reset Code
        
        [HttpPost("verify-code")]
        [Authorize]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            var passwordReset = await _context.PasswordResets
                .Include(pr => pr.User)
                .Where(pr => pr.Code == request.Code && !pr.IsUsed && pr.Expiry > DateTime.UtcNow)
                .OrderByDescending(pr => pr.Expiry)
                .FirstOrDefaultAsync();

            if (passwordReset == null)
            {
                return BadRequest("Invalid or expired reset code.");
            }

            return Ok(new { Email = passwordReset.User!.Email, Message = "Code verified successfully." });
        }

        // Reset Password
        
        [HttpPost("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            var passwordReset = await _context.PasswordResets
                .Include(pr => pr.User)
                .Where(pr => !pr.IsUsed && pr.Expiry > DateTime.UtcNow)
                .OrderByDescending(pr => pr.Expiry)
                .FirstOrDefaultAsync();

            if (passwordReset == null)
            {
                return BadRequest("No valid reset code found. Please request a new code.");
            }

            var user = passwordReset.User!;
            user.PasswordHash = _authService.HashPassword(request.NewPassword);
            passwordReset.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok("Password reset successfully.");
        }

        // Get Profile
        
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user");
            }
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                user.FullName,
                user.Email,
                
                user.ProfilePictureUrl
            });
        }




        
        [HttpPost("upload-profile-image")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            try
            {
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user ID" });
                }

                
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Invalid file type" });

                if (file.Length > 5 * 1024 * 1024) // 5MB
                    return BadRequest(new { message = "File too large" });

                
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var filePath = Path.Combine(uploadsFolder, fileName);

                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound(new { message = "User not found" });

                user.ProfilePictureUrl = $"/uploads/{fileName}";
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Image uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Upload failed", error = ex.Message });
            }
        }



       
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user ID" });
                }

                
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                
                if (!string.IsNullOrEmpty(request.Email))
                {
                    request.Email = request.Email.Trim();
                    if (!new EmailAddressAttribute().IsValid(request.Email))
                    {
                        return BadRequest(new { message = "Invalid email format" });
                    }

                    if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
                    {
                        return BadRequest(new { message = "Email already in use" });
                    }
                    user.Email = request.Email;
                }

                
                if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
                {
                    user.FullName = $"{request.FirstName?.Trim()} {request.LastName?.Trim()}".Trim();
                }

                if (!string.IsNullOrEmpty(request.Phone))
                {
                    user.Phone = request.Phone.Trim();
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Profile updated successfully"
                    
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Update failed", error = ex.Message });
            }
        }


        
    }
}