namespace ProjectManagement_Mirela.Dto.Project
{
    public class ProjectDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TeamId { get; set; }
    }
}
