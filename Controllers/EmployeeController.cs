using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement_Mirela.Data;
using ProjectManagement_Mirela.Dto.Comments;
using ProjectManagement_Mirela.Models;
using System.Security.Claims;

namespace ProjectManagement_Mirela.Controllers
{
    [Route("Employee")]
    [Authorize(Roles = "Employee")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Employee> _userManager;

        public EmployeeController(AppDbContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
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
        public IActionResult EmployeeProjects()
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
        [HttpPost("add-comment")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = await _context.Users
                .Where(u => u.UserName == username)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found.");
            }

            var comment = new Comment
            {
                ProjectId = commentDto.ProjectId,
                EmployeeId = userId,
                Content = commentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }
        [HttpGet("add-comment")]
        public IActionResult AddComment()
        {
            return View();
        }
        [HttpGet("assigned-projects")]
        public async Task<IActionResult> GetAssignedProjects()
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

            var teamId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.TeamId)
                .FirstOrDefaultAsync();

            if (teamId == null)
            {
                return NotFound("User is not assigned to a team.");
            }
            var projects = await _context.Projects
                .Where(p => p.TeamId == teamId)
                .Select(p => new
                {
                    p.ProjectId,
                    p.Name
                })
                .ToListAsync();

            return Ok(projects);
        }
    }
}
