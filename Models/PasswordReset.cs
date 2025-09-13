namespace SmartLandAPI.Models
{
    public class PasswordReset
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Code { get; set; } = string.Empty; 
        public DateTime Expiry { get; set; }
        public bool IsUsed { get; set; }
        public User? User { get; set; }
    }
}

