using HealthyNutritionApp.Application.Dto.Category;

namespace HealthyNutritionApp.Application.Interfaces.Category
{
    public interface ICategoryService
    {
        Task CreateCategoryAsync(CreateCategoryDto categoryDto);
        Task DeleteCategoryAsync(string id);
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync(CategoryFilterDto categoryFilterDto, int offset = 1, int limit = 10);
        Task<CategoryDto> GetCategoryByIdAsync(string id);
        Task UpdateCategoryAsync(string id, UpdateCategoryDto categoryDto);
    }
}
