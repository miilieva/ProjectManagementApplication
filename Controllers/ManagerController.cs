using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement_Mirela.Data;
using ProjectManagement_Mirela.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagement_Mirela.Controllers
{
    [Authorize(Roles ="Manager")]
    [Route("Manager")]
    [ApiController]
    public class ManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Employee> _userManager;

        public ManagerController(AppDbContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
            
        }
        [HttpGet("options")]
        public IActionResult ManagerOptions()
        {
            return View("ManagerOptions");
        }
        [HttpGet]
        [HttpGet("projects")]
        public async Task<IActionResult> Get()
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = await _context.Users
        .Where(u => u.UserName == username)
        .Select(u => u.Id)
        .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found.");
            }
            var employee = await _userManager.FindByIdAsync(userId);

            if (employee == null || employee.TeamId == null)
            {
                return NotFound("User or team not found.");
            }
            var projects = await _context.Projects
                                         .Where(p => p.TeamId == employee.TeamId)
                                         .ToListAsync();

            if (projects == null || projects.Count == 0)
            {
                return NotFound("No projects found for the user's team.");
            }
            return Ok(projects);
        }
        [HttpGet("view-projects")]
        public IActionResult ManagerProjects()
        {
            return View();
        }
        [HttpGet("view-comments/{projectId}")]
        public async Task<IActionResult> ViewComments(int projectId)
        {
            var comments = await _context.Comments
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.CreatedAt) 
            .Include(c => c.Employee)
            .ToListAsync();
            return View("~/Views/Comments/ViewComments.cshtml", comments);
        }
    }
}
