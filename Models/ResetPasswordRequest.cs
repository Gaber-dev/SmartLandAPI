namespace SmartLandAPI.Models
{
    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
