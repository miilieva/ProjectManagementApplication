using ProjectManagement_Mirela.Dto.User;

namespace ProjectManagement_Mirela.Dto.Team
{
    public class EditTeamDto
    {
        public string Name { get; set; }
        public int TeamId { get; set; }
        public List<UserDto> Usernames { get; set; } 
        public List<string> AvailableUsers { get; set; } 
    }
}
