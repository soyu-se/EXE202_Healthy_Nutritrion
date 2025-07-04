﻿using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Dto.Review;

namespace HealthyNutritionApp.Application.Interfaces.Review
{
    public interface IReviewService
    {
        Task<bool> CheckBoughtProduct(string userId, string productId);
        Task DeleteReviewAsync(string reviewId);
        Task<ReviewDto> GetReviewByIdAsync(string reviewId);
        Task<PaginatedResult<ReviewDto>> GetReviewsAsync(ReviewFilterDto reviewFilterDto, int pageIndex = 1, int limit = 10);
        Task<PaginatedResult<ReviewDto>> GetReviewsByProductIdAsync(string productId, int pageIndex = 1, int limit = 10);
        Task<PaginatedResult<ReviewDto>> GetReviewsByUserIdAsync(string userId, int pageIndex = 1, int limit = 10);
        Task PostReviewAsync(CreateReviewDto createReviewDto);
        Task UpdateReviewAsync(string id, UpdateReviewDto updateReviewDto);
    }
}
