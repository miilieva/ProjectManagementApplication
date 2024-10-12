namespace ProjectManagement_Mirela.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public Employee Employee { get; set; } 
        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
