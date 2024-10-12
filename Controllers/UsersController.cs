using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement_Mirela.Models;
using ProjectManagement_Mirela.Dto.Account;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagement_Mirela.Controllers
{
    [Authorize(Roles = "Manager")]
    [Route("Users")]
    public class UsersController : Controller
    {
            private readonly UserManager<Employee> _userManager;
            public UsersController(UserManager<Employee> userManager)
            {
                _userManager = userManager;
            }
            [HttpGet]
            public async Task<IActionResult> GetUsers()
            {
                var employees = await _userManager.Users.ToListAsync();
                return View(employees);
            }

            [HttpGet("create")]
            public IActionResult Create()
            {
                return View(); 
            }
            [HttpPost("create")]
            public async Task<IActionResult> Create(RegisterDto model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                var employee = new Employee
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(employee, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index"); 
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
        
    }
}
