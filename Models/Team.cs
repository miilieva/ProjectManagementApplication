namespace ProjectManagement_Mirela.Models
{
    public class Team
    {
        public int TeamId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Employee> Employees { get; set; } 
        public ICollection<Project> Projects { get; set; } 



    }
}
