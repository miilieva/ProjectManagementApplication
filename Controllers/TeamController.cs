using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement_Mirela.Models;
using ProjectManagement_Mirela.Dto.Team;
using Microsoft.EntityFrameworkCore;

using ProjectManagement_Mirela.Data;
using ProjectManagement_Mirela.Dto.User;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectManagement_Mirela.Controllers
{
    [Route("Teams")]
    [ApiController]
    public class TeamsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Employee> _userManager;

        public TeamsController(AppDbContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _context.Teams.ToListAsync();
            return View(teams); 
        }
        
        [HttpPost("assign")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AssignEmployeeToTeam([FromBody] AssignEmployeeToTeamDto model)
        {
            var team = await _context.Teams.FindAsync(model.TeamId);
            if (team == null)
                return BadRequest("Team not found");

            var employee = await _userManager.FindByNameAsync(model.Username);
            if (employee == null)
                return BadRequest("Employee not found");

            employee.TeamId = model.TeamId;
            _context.Users.Update(employee); 
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee assigned to team successfully" });
        }
        [HttpPost("add-user")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddUserToTeam([FromBody] AddRemoveUserDto model)
        {
            var team = await _context.Teams.FindAsync(model.TeamId);
            if (team == null)
                return BadRequest("Team not found");

            var employee = await _userManager.FindByNameAsync(model.Username);
            if (employee == null)
                return BadRequest("Employee not found");

            employee.TeamId = model.TeamId;
            _context.Users.Update(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee added to team successfully" });
        }
        [HttpPost("remove-user")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RemoveUserFromTeam([FromBody] AddRemoveUserDto model)
        {
            var team = await _context.Teams.FindAsync(model.TeamId);
            if (team == null)
                return BadRequest("Team not found");

            var employee = await _userManager.FindByNameAsync(model.Username);
            if (employee == null)
                return BadRequest("Employee not found");

            employee.TeamId = null;
            _context.Users.Update(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee removed from team successfully" });
        }
        [HttpGet("edit/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound("Team not found");
            }

            var availableUsers = await _userManager.Users
                                .Where(u => u.TeamId == null) 
                                .ToListAsync();
            var teamUsers = await _userManager.Users
                .Where(u => u.TeamId == id)
                .Select(u => u.UserName)
                .ToListAsync();

            var model = new EditTeamDto
            {
                Name = team.Name,
                TeamId = team.TeamId,
                AvailableUsers = availableUsers.Select(u => u.UserName).ToList(),
                Usernames = teamUsers.Select(u => new UserDto { Username = u }).ToList()
            };

            return View(model);
        }
        [HttpPost("edit/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditTeam(EditTeamDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var team = await _context.Teams.FindAsync(model.TeamId);
            if (team == null)
            {
                return NotFound("Team not found");
            }

            team.Name = model.Name;
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetTeams");
        }


        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("create")]
        [Authorize(Roles = "Manager")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create([FromForm] CreateTeamDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var team = new Team
            {
                Name = model.Name
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return RedirectToAction("GetTeams");
        }
    }
}
