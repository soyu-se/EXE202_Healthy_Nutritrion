using HealthyNutritionApp.Application.Dto.Blog;
using HealthyNutritionApp.Application.Dto.PaginatedResult;

namespace HealthyNutritionApp.Application.Interfaces.Blog
{
    public interface IBlogService
    {
        Task CreateBlogAsync(CreateBlogDto createBlogDto);
        Task UpdateBlogAsync(string id, UpdateBlogDto updateBlogDto);
        Task DeleteBlogAsync(string id);
        Task<BlogDto> GetBlogByIdAsync(string id);
        Task<PaginatedResult<BlogDto>> GetBlogsAsync(BlogFilterDto blogFilterDto, int pageIndex = 1, int limit = 10);
    }
}
