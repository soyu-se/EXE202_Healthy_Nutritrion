using HealthyNutritionApp.Application.Dto.Account;
using HealthyNutritionApp.Application.Dto.Authentication;
using HealthyNutritionApp.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Authentication
{
    [Route("api/v1/authentication")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class AuthenticationController(IAuthenticationService authentication) : ControllerBase
    {
        private readonly IAuthenticationService _authentication = authentication;

        [AllowAnonymous, HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterAccountDto registerModel)
        {
            await _authentication.CreateAccountAsync(registerModel);
            return Ok(new { message = "Account created successfully" });
        }

        [AllowAnonymous, HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginModel)
        {
            var accessToken = await _authentication.LoginAsync(loginModel);
            return Ok(new { message = "Login Successfully", accessToken });
        }

        [Authorize(Roles = "User,Admin"), HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordModel)
        {
            await _authentication.ChangePasswordAsync(changePasswordModel.NewPassword, changePasswordModel.OldPassword);
            return Ok(new { message = "Change password successfully" });
        }
    }
}
