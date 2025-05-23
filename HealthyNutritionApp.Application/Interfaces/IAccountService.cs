using HealthyNutritionApp.Application.Dto;

namespace HealthyNutritionApp.Application.Interfaces
{
    public interface IAccountService
    {
        Task EditProfileAsync(EditProfileDto editProfileDto);
        Task<UserProfileDto> GetUserProfileAsync();
    }
}
