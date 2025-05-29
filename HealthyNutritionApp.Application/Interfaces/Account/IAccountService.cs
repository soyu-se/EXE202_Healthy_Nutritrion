using HealthyNutritionApp.Application.Dto.Account;

namespace HealthyNutritionApp.Application.Interfaces.Account
{
    public interface IAccountService
    {
        Task EditProfileAsync(EditProfileDto editProfileDto);
        Task<long> GetTotalCountUsersAsync();
        Task<UserProfileDto> GetUserProfileAsync();
    }
}
