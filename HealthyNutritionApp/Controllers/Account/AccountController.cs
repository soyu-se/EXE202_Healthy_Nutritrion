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
    }
}
