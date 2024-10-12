using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement_Mirela.Data;
using ProjectManagement_Mirela.Models;
using Microsoft.EntityFrameworkCore;
using ProjectManagement_Mirela.Dto.Project;


namespace ProjectManagement_Mirela.Controllers
{
    [Route("Projects")]
    [ApiController]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Employee> _userManager;
        public ProjectsController(AppDbContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet("create")]
        [Authorize(Roles = "Manager")]
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost("create")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create([FromBody] ProjectDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TeamId = model.TeamId
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Project created successfully." });
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> GetProjects()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var projects = await _context.Projects
                .Where(p => p.TeamId == user.TeamId)
                .ToListAsync();

            return Ok(projects);
        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("edit/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
               return NotFound();
            }
            var model = new ProjectDto
            {
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                TeamId = project.TeamId
            };
            return View(model); 
        }
        [HttpPost("edit/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id, [FromBody] ProjectDto model)
        {
            if (!ModelState.IsValid)
               return BadRequest(ModelState);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
               return NotFound();
            }

            project.Name = model.Name;
            project.Description = model.Description;
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;
            project.TeamId = model.TeamId;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Project updated successfully." });
        }
    }
}
