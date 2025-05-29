using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Dto.Review;

namespace HealthyNutritionApp.Application.Interfaces.Review
{
    public interface IReviewService
    {
        Task DeleteReviewAsync(string reviewId);
        Task<ReviewDto> GetReviewByIdAsync(string reviewId);
        Task<PaginatedResult<ReviewDto>> GetReviewsAsync(ReviewFilterDto reviewFilterDto, int offset = 1, int limit = 10);
        Task<PaginatedResult<ReviewDto>> GetReviewsByProductIdAsync(string productId, int offset = 1, int limit = 10);
        Task<PaginatedResult<ReviewDto>> GetReviewsByUserIdAsync(string userId, int offset = 1, int limit = 10);
        Task PostReviewAsync(CreateReviewDto createReviewDto);
        Task UpdateReviewAsync(string id, UpdateReviewDto updateReviewDto);
    }
}
