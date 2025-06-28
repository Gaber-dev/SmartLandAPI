using SmartLandAPI.Models;
namespace SmartLandAPI.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
        string GenerateResetCode();

        
        
    }
}
