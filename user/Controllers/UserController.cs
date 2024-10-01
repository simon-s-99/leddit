using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using userIdentityAPI.DTOs;
using userIdentityAPI.Models;
using userIdentityAPI.Services;
using UserService.DTOs;

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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Create a new ApplicationUser with custom properties
            var user = new ApplicationUser
            {
                UserName = registerUserDto.Username,
                Email = registerUserDto.Email,
                DisplayName = registerUserDto.Username, // Default DisplayName is the same as the Username
                Karma = 0
            };

            // Use CreateAsync to hash the password and save the user
            var result = await _userManager.CreateAsync(user, registerUserDto.Password);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "DuplicateUserName"))                
                    return BadRequest("Username is already taken");
                

                if (result.Errors.Any(e => e.Code == "DuplicateEmail"))              
                    return BadRequest("Email is already in use");
                
                return BadRequest(result.Errors);
            }

            // Publish the UserRegistered event without the password
            var userDto = new UserProfileDto { UserId = user.Id, Username = user.UserName, Email = user.Email };
            _producer.SendMessage("UserRegistered", userDto);

            return Ok($"User {user} registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //PasswordSignInAsync for login
            var result = await _signInManager.PasswordSignInAsync(loginUserDto.Username, loginUserDto.Password, false, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            // Get the user to generate a token. We need this token in order to test our [Put}Profile api call
            var user = await _userManager.FindByNameAsync(loginUserDto.Username);
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "User logged in successfully",
                token
            });

        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok("User logged out successfully");
        }

        [HttpGet("profile/{username}")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
            // Find the user by their username
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound("User not found");

            //Create a profile DTO to return
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

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
        {
            // Get the current logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update user fields
            user.DisplayName = updateUserProfileDto.DisplayName;
            user.Bio = updateUserProfileDto.Bio;
            user.ProfilePictureUrl = updateUserProfileDto.ProfilePictureUrl;
            user.DateOfBirth = updateUserProfileDto.DateOfBirth;

            // Save the changes

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Profile updated successfully");
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the currently logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Ensure new password and confirmation match
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
            {
                return BadRequest("Passwords does not match!");
            }

            if (changePasswordDto.NewPassword == changePasswordDto.CurrentPassword)
            {
                return BadRequest("New password cannot be the same as the current password");
            }

            // Change current password to new password
            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description).ToList());
            }

            return Ok("Password changed successfully");
        }

        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            // Get the current logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Delete the user account
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User account deleted successfully");
        }


        private string GenerateJwtToken(ApplicationUser user)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
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
