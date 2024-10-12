namespace ProjectManagement_Mirela.Models
{
    public class UserRole
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int TeamId { get; set; }
    }
}
