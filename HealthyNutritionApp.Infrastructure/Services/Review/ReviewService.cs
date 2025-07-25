﻿using AutoMapper;
using HealthyNutritionApp.Application.Dto.Account;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Dto.Review;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Review;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Domain.Utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HealthyNutritionApp.Infrastructure.Services.Review
{
    public class ReviewService(IUnitOfWork unitOfWork, IMapper mapper) : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<PaginatedResult<ReviewDto>> GetReviewsAsync(ReviewFilterDto reviewFilterDto, int pageIndex = 1, int limit = 10)
        {
            // Phân trang và lấy tất cả đánh giá
            IQueryable<Reviews> query = _unitOfWork.GetCollection<Reviews>().AsQueryable();

            // Kiểm tra nếu có bộ lọc tìm kiếm
            if (!string.IsNullOrEmpty(reviewFilterDto.Comment))
            {
                query = query.Where(r => r.Comment.Contains(reviewFilterDto.Comment, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrEmpty(reviewFilterDto.ProductId))
            {
                query = query.Where(r => r.ProductId == reviewFilterDto.ProductId);
            }

            if (!string.IsNullOrEmpty(reviewFilterDto.UserId))
            {
                query = query.Where(r => r.UserId == reviewFilterDto.UserId);
            }

            // Fix: Check if Rating is not the default value (0) instead of using HasValue
            if (reviewFilterDto.Rating != default)
            {
                query = query.Where(r => r.Rating == reviewFilterDto.Rating);
            }

            // Phân trang
            query = query.Skip((pageIndex - 1) * limit).Take(limit);

            long totalCount = await _unitOfWork.GetCollection<Reviews>()
                .CountDocumentsAsync(r => string.IsNullOrEmpty(reviewFilterDto.Comment) || r.Comment.Contains(reviewFilterDto.Comment, StringComparison.CurrentCultureIgnoreCase) &&
                                          (string.IsNullOrEmpty(reviewFilterDto.ProductId) || r.ProductId == reviewFilterDto.ProductId) &&
                                          (string.IsNullOrEmpty(reviewFilterDto.UserId) || r.UserId == reviewFilterDto.UserId) &&
                                          (reviewFilterDto.Rating == default || r.Rating == reviewFilterDto.Rating));

            // Thực hiện truy vấn và chuyển đổi kết quả sang danh sách ReviewDto
            IEnumerable<Reviews> reviews = await query.ToListAsync() ?? throw new NotFoundCustomException("No reviews found");

            // Get all user IDs from the reviews
            List<string> userIds = reviews.Select(r => r.UserId).Distinct().ToList();

            // Fetch all users with these IDs
            List<Users> users = await _unitOfWork.GetCollection<Users>()
                .Find(u => userIds.Contains(u.Id))
                .ToListAsync();

            // Create a dictionary for quick user lookup
            Dictionary<string, Users> usersDictionary = users.ToDictionary(u => u.Id);

            // Map reviews to ReviewWithUserDto
            IEnumerable<ReviewDto> reviewsWithUserDto = reviews.Select(review =>
            {
                // Map the review to ReviewDto first
                ReviewDto reviewDto = _mapper.Map<ReviewDto>(review);

                // Create the ReviewWithUserDto
                ReviewDto reviewWithUserDto = new()
                {
                    Id = reviewDto.Id,
                    //UserId = reviewDto.UserId,
                    ProductId = reviewDto.ProductId,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    CreatedAt = reviewDto.CreatedAt,
                    UpdatedAt = reviewDto.UpdatedAt
                };

                // Add user information if available
                if (usersDictionary.TryGetValue(review.UserId, out Users? user))
                {
                    UserProfileDto userProfileDto = new()
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Image = user.Image,
                        Role = user.Role,
                    };

                    reviewWithUserDto.User = userProfileDto;
                }

                return reviewWithUserDto;
            });

            return new PaginatedResult<ReviewDto>
            {
                Items = reviewsWithUserDto,
                TotalCount = totalCount,
            };
        }

        public async Task<ReviewDto> GetReviewByIdAsync(string reviewId)
        {
            // Lấy đánh giá theo ID
            Reviews? review = await _unitOfWork.GetCollection<Reviews>().Find(r => r.Id == reviewId).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Review not found");
            // Chuyển đổi sang ReviewDto
            ReviewDto reviewDto = _mapper.Map<ReviewDto>(review);
            return reviewDto;
        }

        public async Task<PaginatedResult<ReviewDto>> GetReviewsByProductIdAsync(string productId, int pageIndex = 1, int limit = 10)
        {
            // Lấy tất cả đánh giá theo ProductId
            IQueryable<Reviews> query = _unitOfWork.GetCollection<Reviews>().AsQueryable()
                .Where(r => r.ProductId == productId);

            // Phân trang
            query = query.Skip((pageIndex - 1) * limit).Take(limit);

            long totalCount = await _unitOfWork.GetCollection<Reviews>()
                .CountDocumentsAsync(r => r.ProductId == productId);

            // Thực hiện truy vấn và chuyển đổi kết quả sang danh sách ReviewDto
            IEnumerable<Reviews> reviews = await query.ToListAsync() ?? throw new NotFoundCustomException("No reviews found for this product");
            IEnumerable<ReviewDto> reviewsDto = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

            return new PaginatedResult<ReviewDto>
            {
                Items = reviewsDto,
                TotalCount = totalCount,
            };
        }

        public async Task<PaginatedResult<ReviewDto>> GetReviewsByUserIdAsync(string userId, int pageIndex = 1, int limit = 10)
        {
            // Lấy tất cả đánh giá theo UserId
            IQueryable<Reviews> query = _unitOfWork.GetCollection<Reviews>().AsQueryable()
                .Where(r => r.UserId == userId);
            // Phân trang
            query = query.Skip((pageIndex - 1) * limit).Take(limit);

            long totalCount = await _unitOfWork.GetCollection<Reviews>()
                .CountDocumentsAsync(r => r.UserId == userId);

            // Thực hiện truy vấn và chuyển đổi kết quả sang danh sách ReviewDto
            IEnumerable<Reviews> reviews = await query.ToListAsync() ?? throw new NotFoundCustomException("No reviews found for this user");
            IEnumerable<ReviewDto> reviewsDto = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

            return new PaginatedResult<ReviewDto>
            {
                Items = reviewsDto,
                TotalCount = totalCount,
            }; ;
        }

        public async Task<bool> CheckBoughtProduct(string userId, string productId)
        {
            // Fix: Replace 'Product' with 'Items' as per the Orders class definition
            long count = await _unitOfWork.GetCollection<Orders>()
                .CountDocumentsAsync(o => o.UserId == userId && o.Items.Any(p => p.ProductId == productId) && o.Status == "SHIPPED");
            return count > 0;
        }

        public async Task PostReviewAsync(CreateReviewDto createReviewDto)
        {
            double rating = await _unitOfWork.GetCollection<Products>()
                .Find(r => r.Id == createReviewDto.ProductId)
                .Project(r => r.Rating)
                .FirstOrDefaultAsync();
            
            Reviews review = new()
            {
                UserId = createReviewDto.UserId,
                ProductId = createReviewDto.ProductId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                CreatedAt = TimeControl.GetUtcPlus7Time(),
                UpdatedAt = null
            };

            long reviewCount = await _unitOfWork.GetCollection<Reviews>()
                .CountDocumentsAsync(r => r.ProductId == createReviewDto.ProductId);

            UpdateDefinition<Products> updateDefinition = Builders<Products>.Update
                .Set(p => p.Rating, (rating * reviewCount + createReviewDto.Rating) / (reviewCount + 1))
                .Set(p => p.ReviewCount, reviewCount + 1)
                .Set(p => p.UpdatedAt, TimeControl.GetUtcPlus7Time());

            // Cập nhật sản phẩm với đánh giá mới
            UpdateResult updateResult = await _unitOfWork.GetCollection<Products>()
                .UpdateOneAsync(p => p.Id == createReviewDto.ProductId, updateDefinition);

            await _unitOfWork.GetCollection<Reviews>().InsertOneAsync(review);
        }

        public async Task UpdateReviewAsync(string id, UpdateReviewDto updateReviewDto)
        {
            // Get the existing review first to confirm it exists
            Reviews? existingReview = await _unitOfWork.GetCollection<Reviews>()
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (existingReview == null)
            {
                throw new NotFoundCustomException("Review not found");
            }

            // Build update definition dynamically based on provided values
            UpdateDefinitionBuilder<Reviews> updateBuilder = Builders<Reviews>.Update;
            List<UpdateDefinition<Reviews>> updates = new List<UpdateDefinition<Reviews>>();

            // For Rating (double type), check if it's a valid rating value (typically 1-5)
            if (updateReviewDto.Rating >= 1 && updateReviewDto.Rating <= 5)
            {
                updates.Add(updateBuilder.Set(r => r.Rating, updateReviewDto.Rating));
            }

            // Only update Comment if it's not null
            if (updateReviewDto.Comment != null)
            {
                updates.Add(updateBuilder.Set(r => r.Comment, updateReviewDto.Comment));
            }

            // Always update the UpdatedAt timestamp when making changes
            updates.Add(updateBuilder.Set(r => r.UpdatedAt, TimeControl.GetUtcPlus7Time()));

            if (updates.Count > 1) // > 1 because we always add UpdatedAt
            {
                UpdateDefinition<Reviews> updateDefinition = updateBuilder.Combine(updates);

                UpdateResult result = await _unitOfWork.GetCollection<Reviews>()
                    .UpdateOneAsync(r => r.Id == id, updateDefinition);

                if (result.ModifiedCount == 0)
                {
                    throw new NotFoundCustomException("No changes were made to the review");
                }
            }
            else
            {
                throw new BadRequestCustomException("No valid properties were provided for update");
            }
        }

        public async Task DeleteReviewAsync(string reviewId)
        {
            // Xóa đánh giá theo Id
            DeleteResult result = await _unitOfWork.GetCollection<Reviews>()
                .DeleteOneAsync(r => r.Id == reviewId);

            if (result.DeletedCount == 0)
            {
                throw new NotFoundCustomException("Review not found");
            }
        }
    }
}
