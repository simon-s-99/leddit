using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using userIdentityAPI.Models;
using userIdentityAPI.Services;
using UserService.DTOs;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RabbitMQProducer _producer;

        public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RabbitMQProducer producer)
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
                DateOfBirth = user.DateOfBirth
            };

            return Ok(profile);
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

            return Ok("User logged in successfully");
        }
    }
}
