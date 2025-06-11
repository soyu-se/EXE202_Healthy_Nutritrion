using HealthyNutritionApp.Application.Dto.Account;
using HealthyNutritionApp.Application.Interfaces.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Account
{
    [Route("api/v1/accounts")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        private readonly IAccountService _accountService = accountService;

        [Authorize(Roles = "User, Admin"), HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var result = await _accountService.GetUserProfileAsync();
            return Ok(new { message = "User profile retrieved successfully", result });
        }

        [Authorize(Roles = "User, Admin"), HttpPut("profile")]
        public async Task<IActionResult> EditUserProfile(EditProfileDto editProfileDto)
        {
            await _accountService.EditProfileAsync(editProfileDto);
            return Ok(new { message = "User profile updated successfully" });
        }

        [Authorize(Roles = "Admin"), HttpGet]
        public async Task<IActionResult> GetAllUsers(int pageIndex = 1, int limit = 10)
        {
            var result = await _accountService.GetUsersAsync(pageIndex, limit);
            return Ok(new { message = "User accounts retrieved successfully", result });
        }

        [AllowAnonymous, HttpGet("profile/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _accountService.GetUserProfileByIdAsync(id);
            return Ok(new { message = "User account retrieved successfully", result });
        }

        [Authorize(Roles = "Admin"), HttpPost]
        public async Task<IActionResult> CreateAccount(CreateUserDto createUserDto)
        {
            await _accountService.CreateUserAsync(createUserDto);
            return Ok(new { message = "Account created successfully" });
        }
    }
}
