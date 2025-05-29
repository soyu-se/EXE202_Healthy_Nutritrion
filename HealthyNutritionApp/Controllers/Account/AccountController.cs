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

        [Authorize(Roles = "User"), HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var result = await _accountService.GetUserProfileAsync();
            return Ok(new { message = "User profile retrieved successfully", result });
        }

        [Authorize(Roles = "Admin"), HttpGet]
        public async Task<IActionResult> GetAllUsers(int pageIndex = 1, int limit = 10)
        {
            var result = await _accountService.GetUsersAsync(pageIndex, limit);
            return Ok(new { message = "User accounts retrieved successfully", result });
        }

        [Authorize(Roles = "Admin"), HttpPost]
        public async Task<IActionResult> CreateAccount(CreateUserDto createUserDto)
        {
            await _accountService.CreateUserAsync(createUserDto);
            return Ok(new { message = "Account created successfully" });
        }
    }
}
