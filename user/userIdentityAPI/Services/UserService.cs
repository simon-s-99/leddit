using Microsoft.AspNetCore.Identity;
using UserService.DTOs;

namespace userIdentityAPI.Services
{
    public class UserService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UserService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            var user = new IdentityUser { UserName = registerUserDto.Username, Email = registerUserDto.Email };
            await _userManager.CreateAsync(user, registerUserDto.Password);
        }
    }
}
