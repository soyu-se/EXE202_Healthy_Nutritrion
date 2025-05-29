using AutoMapper;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Dto.Review;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Review;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Domain.Utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HealthyNutritionApp.Infrastructure.Implements.Review
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
            IEnumerable<ReviewDto> reviewsDto = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

            return new PaginatedResult<ReviewDto>
            {
                Items = reviewsDto,
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

        public async Task PostReviewAsync(CreateReviewDto createReviewDto)
        {
            Reviews review = new()
            {
                UserId = createReviewDto.UserId,
                ProductId = createReviewDto.ProductId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                CreatedAt = TimeControl.GetUtcPlus7Time()
            };

            await _unitOfWork.GetCollection<Reviews>().InsertOneAsync(review);
        }

        public async Task UpdateReviewAsync(string id, UpdateReviewDto updateReviewDto)
        {
            // Cập nhật đánh giá theo ID
            UpdateDefinition<Reviews> updateDefinition = Builders<Reviews>.Update
                .Set(r => r.Rating, updateReviewDto.Rating)
                .Set(r => r.Comment, updateReviewDto.Comment)
                .Set(r => r.UpdatedAt, TimeControl.GetUtcPlus7Time());

            UpdateResult result = await _unitOfWork.GetCollection<Reviews>()
                .UpdateOneAsync(r => r.Id == id, updateDefinition);

            if (result.ModifiedCount == 0)
            {
                throw new NotFoundCustomException("Review not found or no changes made");
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
