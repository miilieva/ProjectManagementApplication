using Microsoft.AspNetCore.Identity;

namespace ProjectManagement_Mirela.Models
{
    public class Employee : IdentityUser
    {
        public int? TeamId { get; set; }
        public Team Team { get; set; }
    }
}
