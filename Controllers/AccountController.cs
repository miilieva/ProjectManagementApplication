using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement_Mirela.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProjectManagement_Mirela.Dto.Account;
using Microsoft.AspNetCore.Authorization;


namespace ProjectManagement_Mirela.Controllers
{
    [Route("Account")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<Employee> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<Employee> _signInManager;
        public AccountController(UserManager<Employee> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AccountController> logger, SignInManager<Employee> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _signInManager = signInManager;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] Login model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }
            _logger.LogInformation($"User ID: {user.Id}");
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!)),
                    SecurityAlgorithms.HmacSha256
                    )
                    );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = true, 
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]))
                };
                Response.Cookies.Append("jwtToken", tokenString, cookieOptions);

                if (userRoles.Contains("Manager"))
                {
                    return RedirectToAction("ManagerOptions", "Manager"); 
                }
                if (userRoles.Contains("Employee"))
                {
                    return RedirectToAction("EmployeeProjects", "Employee");
                }

                return Ok(new { Token = tokenString });
            }
            return Unauthorized();
        }
        
        [HttpPost("get-token")]
        public async Task<IActionResult> GetToken([FromForm] Login model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!)),
                        SecurityAlgorithms.HmacSha256
                    )
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { token = tokenString });
            }

            return Unauthorized();
        }
       
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View(); 
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = new Employee { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    if (!roleResult.Succeeded)
                    {
                        return BadRequest(new { success = false, message = "Role creation failed." });
                    }
                }
                var roleAssignResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (roleAssignResult.Succeeded)
                {
                    return Ok(new { success = true, message = "User registered and role assigned successfully." });
                }

                return BadRequest(new { success = false, message = "Failed to assign role." });
            }
            return BadRequest(new { success = false, message = "User registration failed." });
        }


        [HttpPost("add-role")]
        public async Task<IActionResult> AddRole([FromBody] string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    return Ok(new { message = "Role added succesfully" });
                }
                return BadRequest(result.Errors);
            }
            return BadRequest("Role already exists");
        }
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {

                return BadRequest("User not found");
            }
            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (result.Succeeded)
            {
                return Ok(new { message = "Role assigned succesfully" });
            }
            return BadRequest(result.Errors);
        }
        [HttpPost("logout")]
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); 
            Response.Cookies.Delete("jwtToken");
            return RedirectToAction("Login", "Account");
        }
    }
}
