using HealthyNutritionApp.Application.Dto.Account;
using HealthyNutritionApp.Application.Dto.PaginatedResult;

namespace HealthyNutritionApp.Application.Interfaces.Account
{
    public interface IAccountService
    {
        Task CreateUserAsync(CreateUserDto createUserDto);
        Task DeleteUserAsync(string userId);
        Task EditProfileAsync(EditProfileDto editProfileDto);
        Task<long> GetTotalCountUsersAsync();
        Task<UserProfileDto> GetUserProfileAsync();
        Task<PaginatedResult<UserAccountDto>> GetUsersAsync(int pageIndex, int limit);
    }
}
