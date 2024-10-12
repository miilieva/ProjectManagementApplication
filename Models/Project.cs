using Microsoft.AspNetCore.Identity;

namespace ProjectManagement_Mirela.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; } 
    }
}
