using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using userIdentityAPI.DTOs;
using userIdentityAPI.Services;
using LedditModels;
using System.IdentityModel.Tokens.Jwt;
using UserService.DTOs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RabbitMQProducer _producer;

        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RabbitMQProducer producer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _producer = producer;
        }

        // POST: api/user/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = registerUserDto.Username,
                Email = registerUserDto.Email,
                DisplayName = registerUserDto.Username,
                Karma = 0
            };

            var result = await _userManager.CreateAsync(user, registerUserDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            // Publish the event to RabbitMQ
            _producer.NotifyUserEvent("register-user", new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email
            });

            return Ok($"User {user.UserName} registered successfully");
        }

        // POST: api/user/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _signInManager.PasswordSignInAsync(loginUserDto.Username, loginUserDto.Password, false, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            var user = await _userManager.FindByNameAsync(loginUserDto.Username);

            // Generate a JWT token for the logged in user
            var token = GenerateJwtToken(user);

           
            return Ok(new { message = "User logged in successfully", token });
        }


        // POST: api/user/logout
        // The logout method cannot invalidate a JWT token because JWTs are stateless.
        // Once a JWT is issued, the server has no control over it until it naturally expires.
        // We could implement a solution where the token gets blacklisted when logged out,
        // but this would just make testing the API a bit more complicated
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok("User logged out successfully");
        }


        // GET: api/user/profile/{username}
        // Method that fetches an users info
        [HttpGet("profile/{username}")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound("User not found");

            var profile = new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                DateOfBirth = user.DateOfBirth,
                Karma = user.Karma
            };
            return Ok(profile);
        }


       // Updates the profile information of the currently authenticated user
       // PUT: api/user/profile
       [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            user.DisplayName = updateUserProfileDto.DisplayName;
            user.Bio = updateUserProfileDto.Bio;
            user.ProfilePictureUrl = updateUserProfileDto.ProfilePictureUrl;
            user.DateOfBirth = updateUserProfileDto.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Publish the event to RabbitMQ
            _producer.NotifyUserEvent("update-user", updateUserProfileDto);

            return Ok("Profile updated successfully");
        }

        // Method to retrieve the user ID for the comment microservice
        // GET: api/user/userid/{userId}
        [HttpGet("userid/{userId}")]
        public async Task<IActionResult> GetUserId(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            return Ok(new { UserId = user.Id });
        }


        // POST: api/user/change-password
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                return BadRequest("Passwords do not match!");

            if (changePasswordDto.NewPassword == changePasswordDto.CurrentPassword)
                return BadRequest("New password cannot be the same as the current password");

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description).ToList());

            return Ok("Password changed successfully");
        }

        // DELETE: api/user/delete-account
        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Publish the event to RabbitMQ
            _producer.NotifyUserEvent("delete-user", new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName
            });

            return Ok("User account deleted successfully");
        }

        [HttpGet("crash")]
        public IActionResult CrashApp()
        {
            Environment.Exit(1);
            return Ok("App crashed successfully");
        }

        // Generates a JWT token for the specified user to authorize API requests
        private string GenerateJwtToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ThisIsAVerySecureAndLongEnoughSecretKey12345");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "UserProfile",
                Audience = "UserProfile"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
