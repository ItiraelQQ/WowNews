using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using WowNewsAPI.Data;
using WowNewsAPI.Models;
using System.Security.Claims;

namespace WowNewsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly TokenService _tokenService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            TokenService tokenService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                var user = new ApplicationUser { UserName = model.Username, AvatarUrl = "/uploads/avatars/default.jpg", };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "User registered successfully." });
                }

                return BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration.");
                return StatusCode(500, new { Message = "An error occurred during registration." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid input data." });
            }

            try
            {
                _logger.LogInformation("Attempting login for user: {Username}", model.Username);

                
                var user = await _userManager.FindByNameAsync(model.Username);

                
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Username}", model.Username);
                    return Unauthorized(new { Message = "Invalid username or password." });
                }

                
                var result = await _userManager.CheckPasswordAsync(user, model.Password);

                if (!result)
                {
                    _logger.LogWarning("Invalid password attempt for user: {Username}", model.Username);
                    return Unauthorized(new { Message = "Invalid username or password." });
                }

                
                var roles = await _userManager.GetRolesAsync(user);

                
                string token = _tokenService.GenerateJwtToken(user.UserName, roles, user.Id);

                _logger.LogInformation($"User {User.Identity.Name} logged in successfully. Token generated.", model.Username);

                // Return the token
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", model.Username);
                return StatusCode(500, new { Message = "An error occurred during login." });
            }
        }



        [Authorize]
        [HttpGet("check-login-status")]
        public IActionResult CheckLoginStatus()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var username = User.Identity.Name;
                    return Ok(new { isLoggedIn = true, username });
                }

                return Ok(new { isLoggedIn = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking login status.");
                return StatusCode(500, new { Message = "An error occurred while checking login status." });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                var profileData = new
                {
                    Username = user.UserName,
                    AvatarUrl = user.AvatarUrl,
                    
                };

                return Ok(profileData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile.");
                return StatusCode(500, new { Message = "An error occurred while retrieving profile." });
            }
        }

        [Authorize]
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file selected.");
                }

                var username = User.Identity.Name;
                var user = await _userManager.FindByNameAsync(username);

                if (user == null)
                {
                    return NotFound();
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads", "avatars");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                user.AvatarUrl = $"/uploads/avatars/{fileName}";
                await _userManager.UpdateAsync(user);

                return Ok(new { AvatarUrl = user.AvatarUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar.");
                return StatusCode(500, new { Message = "An error occurred while uploading avatar." });
            }
        }
    }

}
