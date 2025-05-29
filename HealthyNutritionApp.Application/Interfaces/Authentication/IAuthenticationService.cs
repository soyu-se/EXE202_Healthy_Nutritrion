using HealthyNutritionApp.Application.Dto.Authentication;

namespace HealthyNutritionApp.Application.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        Task ChangePasswordAsync(string newPassword, string oldPassword);
        Task CreateAccountAsync(RegisterAccountDto registerUser);
        Task<AuthenticationTokenDto> LoginAsync(LoginDto loginUser);
    }
}
