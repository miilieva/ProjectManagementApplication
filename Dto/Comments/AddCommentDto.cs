using ProjectManagement_Mirela.Models;

namespace ProjectManagement_Mirela.Dto.Comments
{
    public class AddCommentDto
    {
        public string Content { get; set; }
        public int ProjectId { get; set; }
    }
}
