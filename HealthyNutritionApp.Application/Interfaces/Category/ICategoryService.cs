using HealthyNutritionApp.Application.Dto.Category;
using HealthyNutritionApp.Application.Dto.PaginatedResult;

namespace HealthyNutritionApp.Application.Interfaces.Category
{
    public interface ICategoryService
    {
        Task CreateCategoryAsync(CreateCategoryDto categoryDto);
        Task DeleteCategoryAsync(string id);
        Task<PaginatedResult<CategoryDto>> GetCategoriesAsync(CategoryFilterDto categoryFilterDto, int offset = 1, int limit = 10);
        Task<CategoryDto> GetCategoryByIdAsync(string id);
        Task UpdateCategoryAsync(string id, UpdateCategoryDto categoryDto);
    }
}
