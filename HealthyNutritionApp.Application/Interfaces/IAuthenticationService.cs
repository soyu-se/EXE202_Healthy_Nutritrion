
using HealthyNutritionApp.Application.Dto;

namespace HealthyNutritionApp.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task ChangePasswordAsync(string newPassword, string oldPassword);
        Task CreateAccountAsync(RegisterAccountDto registerUser);
        Task<AuthenticationTokenDto> LoginAsync(LoginDto loginUser);
    }
}
