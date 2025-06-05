using AutoMapper;
using CloudinaryDotNet.Actions;
using HealthyNutritionApp.Application.Dto.Blog;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Blog;
using HealthyNutritionApp.Application.ThirdPartyServices.Cloudinary;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Domain.Enums;
using HealthyNutritionApp.Domain.Utils;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HealthyNutritionApp.Infrastructure.Services.Blog
{
    public class BlogService(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService, IMapper mapper) : IBlogService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
        private readonly IMapper _mapper = mapper;

        // Add your service methods here, e.g. for creating, updating, deleting, and retrieving blogs
        public async Task CreateBlogAsync(CreateBlogDto createBlogDto)
        {
            bool blogExists = await _unitOfWork.GetCollection<Blogs>()
                .Find(b => b.Title == createBlogDto.Title)
                .AnyAsync();
            if (blogExists)
            {
                throw new BadRequestCustomException($"Blogs already exists");
            }

            // Upload ảnh lên Cloudinary
            List<string> imageUrls = [];
            if (createBlogDto.ImageBlog != null && createBlogDto.ImageBlog.Any())
            {
                foreach (IFormFile image in createBlogDto.ImageBlog)
                {
                    ImageUploadResult result = _cloudinaryService.UploadImage(image, ImageTag.Blog);
                    imageUrls.Add(result.SecureUrl.AbsoluteUri);
                }
            }

            Domain.Entities.Blogs blog = new()
            {
                Title = createBlogDto.Title,
                Content = createBlogDto.Content,
                Excerpt = createBlogDto.Excerpt,
                Tags = createBlogDto.Tags,
                Images = imageUrls,
                CreatedAt = TimeControl.GetUtcPlus7Time(),
                UpdatedAt = null // Assuming this is set later when the blog is updated
            };

            await _unitOfWork.GetCollection<Blogs>().InsertOneAsync(blog);

            return;
        }

        public async Task UpdateBlogAsync(string id, UpdateBlogDto updateBlogDto)
        {
            Domain.Entities.Blogs existingBlog = await _unitOfWork.GetCollection<Blogs>()
                .Find(b => b.Id == id)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"No blog found with ID: {id}");

            UpdateDefinitionBuilder<Blogs> updateBuilder = Builders<Blogs>.Update;

            List<UpdateDefinition<Blogs>> updates =
            [
                updateBuilder.Set(b => b.Title, updateBlogDto.Title),
                updateBuilder.Set(b => b.Content, updateBlogDto.Content),
                updateBuilder.Set(b => b.Excerpt, updateBlogDto.Excerpt),
                updateBuilder.Set(b => b.Tags, updateBlogDto.Tags),
                updateBuilder.Set(b => b.UpdatedAt, TimeControl.GetUtcPlus7Time()),
            ];

            if (updateBlogDto.ImageBlog != null && updateBlogDto.ImageBlog.Any())
            {
                List<string> imageUrls = [];
                foreach (IFormFile image in updateBlogDto.ImageBlog)
                {
                    ImageUploadResult result = _cloudinaryService.UploadImage(image, ImageTag.Blog);
                    imageUrls.Add(result.SecureUrl.AbsoluteUri);
                }

                updates.Add(updateBuilder.Set(b => b.Images, imageUrls));
            }

            UpdateDefinition<Blogs> updateDefinition = updateBuilder.Combine(updates);

            await _unitOfWork.GetCollection<Blogs>()
                .UpdateOneAsync(b => b.Id == id, updateDefinition);
        }

        public async Task DeleteBlogAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Blogs ID cannot be null or empty.", nameof(id));
            }

            FilterDefinition<Blogs> filter = Builders<Blogs>.Filter.Eq(b => b.Id, id);
            DeleteResult result = await _unitOfWork.GetCollection<Blogs>().DeleteOneAsync(filter);
            if (result.DeletedCount == 0)
            {
                throw new KeyNotFoundException($"No blog found with ID: {id}");
            }
        }

        public async Task<BlogDto> GetBlogByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Blogs ID cannot be null or empty.", nameof(id));
            }

            FilterDefinition<Blogs> filter = Builders<Blogs>.Filter.Eq(b => b.Id, id);
            Domain.Entities.Blogs blog = await _unitOfWork.GetCollection<Blogs>().Find(filter).FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"No blog found with ID: {id}");

            return new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Excerpt = blog.Excerpt,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt
            };
        }

        public async Task<PaginatedResult<BlogDto>> GetBlogsAsync(BlogFilterDto blogFilterDto, int pageIndex = 1, int limit = 10)
        {
            IQueryable<Blogs> query = _unitOfWork.GetCollection<Blogs>().AsQueryable();

            // Kiểm tra nếu có bộ lọc tìm kiếm
            if (!string.IsNullOrEmpty(blogFilterDto.SearchTerm))
            {
                query = query.Where(b => b.Title.Contains(blogFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                                         b.Content.Contains(blogFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase));
            }

            // Lọc theo thẻ
            if (blogFilterDto.Tags != null && blogFilterDto.Tags.Any())
            {
                query = query.Where(b => b.Tags != null && b.Tags.Any(t => blogFilterDto.Tags.Contains(t)));
            }

            // Lọc theo ngày
            if (blogFilterDto.StartDate.HasValue)
            {
                query = query.Where(b => b.CreatedAt >= blogFilterDto.StartDate.Value);
            }

            if (blogFilterDto.EndDate.HasValue)
            {
                query = query.Where(b => b.CreatedAt <= blogFilterDto.EndDate.Value);
            }

            // Phân trang
            query = query.Skip((pageIndex - 1) * limit).Take(limit);

            // Thực thi truy vấn và chuyển đổi sang DTO
            IEnumerable<Blogs> blogs = await query.ToListAsync();
            IEnumerable<BlogDto> blogDtos = _mapper.Map<IEnumerable<BlogDto>>(blogs);

            if(!blogDtos.Any())
            {
                return new PaginatedResult<BlogDto>
                {
                    Items = [],
                    TotalCount = 0,
                };
            }

            long totalCount = await _unitOfWork.GetCollection<Blogs>()
                .CountDocumentsAsync(b => (string.IsNullOrEmpty(blogFilterDto.SearchTerm) || b.Title.Contains(blogFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase) || b.Content.Contains(blogFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase)) &&
                                          (!blogFilterDto.Tags.Any() || (b.Tags != null && b.Tags.Any(t => blogFilterDto.Tags.Contains(t)))) &&
                                          (!blogFilterDto.StartDate.HasValue || b.CreatedAt >= blogFilterDto.StartDate.Value) &&
                                          (!blogFilterDto.EndDate.HasValue || b.CreatedAt <= blogFilterDto.EndDate.Value));

            return new PaginatedResult<BlogDto>
            {
                Items = blogDtos,
                TotalCount = totalCount,
            };
        }
    }
}
