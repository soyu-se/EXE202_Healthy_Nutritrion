using AutoMapper;
using CloudinaryDotNet.Actions;
using HealthyNutritionApp.Application.Dto.Blog;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Blog;
using HealthyNutritionApp.Application.Projections.Dto.Blog;
using HealthyNutritionApp.Application.ThirdPartyServices.Cloudinary;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Domain.Enums;
using HealthyNutritionApp.Domain.Utils;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

            // Generate slug from title
            string slug = CreateSlug(createBlogDto.Title);

            // Check if slug already exists
            bool slugExists = await _unitOfWork.GetCollection<Blogs>()
                .Find(b => b.Slug == slug)
                .AnyAsync();

            if (slugExists)
            {
                // Append a unique identifier to make the slug unique
                slug = $"{slug}-{DateTime.Now.Ticks.ToString("x")}";
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

            Blogs blog = new()
            {
                Title = createBlogDto.Title,
                Content = createBlogDto.Content,
                Excerpt = createBlogDto.Excerpt,
                Slug = slug,
                Tags = createBlogDto.Tags,
                Images = imageUrls,
                CreatedAt = TimeControl.GetUtcPlus7Time(),
                UpdatedAt = null // Assuming this is set later when the blog is updated
            };

            await _unitOfWork.GetCollection<Blogs>().InsertOneAsync(blog);

            return;
        }

        public string CreateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Chuyển các ký tự đặc biệt tiếng Việt thành không dấu (bao gồm Đ, đ)
            Dictionary<char, char> vietnameseMap = new()
            {
                {'à','a'},{'á','a'},{'ạ','a'},{'ả','a'},{'ã','a'},
                {'â','a'},{'ầ','a'},{'ấ','a'},{'ậ','a'},{'ẩ','a'},{'ẫ','a'},
                {'ă','a'},{'ằ','a'},{'ắ','a'},{'ặ','a'},{'ẳ','a'},{'ẵ','a'},
                {'è','e'},{'é','e'},{'ẹ','e'},{'ẻ','e'},{'ẽ','e'},
                {'ê','e'},{'ề','e'},{'ế','e'},{'ệ','e'},{'ể','e'},{'ễ','e'},
                {'ì','i'},{'í','i'},{'ị','i'},{'ỉ','i'},{'ĩ','i'},
                {'ò','o'},{'ó','o'},{'ọ','o'},{'ỏ','o'},{'õ','o'},
                {'ô','o'},{'ồ','o'},{'ố','o'},{'ộ','o'},{'ổ','o'},{'ỗ','o'},
                {'ơ','o'},{'ờ','o'},{'ớ','o'},{'ợ','o'},{'ở','o'},{'ỡ','o'},
                {'ù','u'},{'ú','u'},{'ụ','u'},{'ủ','u'},{'ũ','u'},
                {'ư','u'},{'ừ','u'},{'ứ','u'},{'ự','u'},{'ử','u'},{'ữ','u'},
                {'ỳ','y'},{'ý','y'},{'ỵ','y'},{'ỷ','y'},{'ỹ','y'},
                {'đ','d'},
                {'À','A'},{'Á','A'},{'Ạ','A'},{'Ả','A'},{'Ã','A'},
                {'Â','A'},{'Ầ','A'},{'Ấ','A'},{'Ậ','A'},{'Ẩ','A'},{'Ẫ','A'},
                {'Ă','A'},{'Ằ','A'},{'Ắ','A'},{'Ặ','A'},{'Ẳ','A'},{'Ẵ','A'},
                {'È','E'},{'É','E'},{'Ẹ','E'},{'Ẻ','E'},{'Ẽ','E'},
                {'Ê','E'},{'Ề','E'},{'Ế','E'},{'Ệ','E'},{'Ể','E'},{'Ễ','E'},
                {'Ì','I'},{'Í','I'},{'Ị','I'},{'Ỉ','I'},{'Ĩ','I'},
                {'Ò','O'},{'Ó','O'},{'Ọ','O'},{'Ỏ','O'},{'Õ','O'},
                {'Ô','O'},{'Ồ','O'},{'Ố','O'},{'Ộ','O'},{'Ổ','O'},{'Ỗ','O'},
                {'Ơ','O'},{'Ờ','O'},{'Ớ','O'},{'Ợ','O'},{'Ở','O'},{'Ỡ','O'},
                {'Ù','U'},{'Ú','U'},{'Ụ','U'},{'Ủ','U'},{'Ũ','U'},
                {'Ư','U'},{'Ừ','U'},{'Ứ','U'},{'Ự','U'},{'Ử','U'},{'Ữ','U'},
                {'Ỳ','Y'},{'Ý','Y'},{'Ỵ','Y'},{'Ỷ','Y'},{'Ỹ','Y'},
                {'Đ','D'}
            };

            StringBuilder sb = new();

            foreach (char c in text)
            {
                if (vietnameseMap.ContainsKey(c))
                    sb.Append(vietnameseMap[c]);
                else
                    sb.Append(c);
            }

            string result = sb.ToString().ToLowerInvariant();

            // Loại bỏ ký tự đặc biệt không phải chữ/số/dấu cách
            result = Regex.Replace(result, @"[^a-z0-9\s-]", string.Empty);

            // Thay khoảng trắng bằng dấu gạch ngang
            result = Regex.Replace(result, @"\s+", "-");

            // Loại bỏ dấu gạch ngang liên tiếp
            result = Regex.Replace(result, @"-+", "-");

            // Loại bỏ dấu gạch đầu/cuối
            result = result.Trim('-');

            return result;
        }

        public async Task UpdateBlogAsync(string id, UpdateBlogDto updateBlogDto)
        {
            Domain.Entities.Blogs existingBlog = await _unitOfWork.GetCollection<Blogs>()
                .Find(b => b.Id == id)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"No blog found with ID: {id}");

            UpdateDefinitionBuilder<Blogs> updateBuilder = Builders<Blogs>.Update;
            List<UpdateDefinition<Blogs>> updates = [];

            // Only process title update if it's provided and different
            string newSlug = null;
            if (updateBlogDto.Title != null && existingBlog.Title != updateBlogDto.Title)
            {
                // Add title update
                updates.Add(updateBuilder.Set(b => b.Title, updateBlogDto.Title));

                // Generate new slug when title changes
                newSlug = CreateSlug(updateBlogDto.Title);

                // Check if new slug already exists (excluding current blog)
                bool slugExists = await _unitOfWork.GetCollection<Blogs>()
                    .Find(b => b.Slug == newSlug && b.Id != id)
                    .AnyAsync();

                if (slugExists)
                {
                    // Append a unique identifier to make the slug unique
                    newSlug = $"{newSlug}-{DateTime.Now.Ticks.ToString("x")}";
                }

                // Add slug update
                updates.Add(updateBuilder.Set(b => b.Slug, newSlug));
            }

            // Add other updates only if fields are provided
            if (updateBlogDto.Content != null)
            {
                updates.Add(updateBuilder.Set(b => b.Content, updateBlogDto.Content));
            }

            if (updateBlogDto.Excerpt != null)
            {
                updates.Add(updateBuilder.Set(b => b.Excerpt, updateBlogDto.Excerpt));
            }

            if (updateBlogDto.Tags != null)
            {
                updates.Add(updateBuilder.Set(b => b.Tags, updateBlogDto.Tags));
            }

            // Process image updates if provided
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

            // Only perform update if there are changes to make
            if (updates.Count > 0)
            {
                // Always update the UpdatedAt timestamp when changes are made
                updates.Add(updateBuilder.Set(b => b.UpdatedAt, TimeControl.GetUtcPlus7Time()));

                UpdateDefinition<Blogs> updateDefinition = updateBuilder.Combine(updates);

                await _unitOfWork.GetCollection<Blogs>()
                    .UpdateOneAsync(b => b.Id == id, updateDefinition);
            }
            else
            {
                throw new BadRequestCustomException("No fields provided for update");
            }
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
                query = query.Where(b => b.Title.Contains(blogFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase));
            }

            // Lọc theo slug (nếu cần)
            if (!string.IsNullOrEmpty(blogFilterDto.Slug))
            {
                query = query.Where(b => b.Slug == blogFilterDto.Slug);
            }

            // Lọc theo thẻ
            if (blogFilterDto.Tags != null && blogFilterDto.Tags.Any())
            {
                query = query.Where(b => b.Tags != null && b.Tags.Any(t => blogFilterDto.Tags.Contains(t)));
            }

            //// Lọc theo ngày
            //if (blogFilterDto.StartDate.HasValue)
            //{
            //    query = query.Where(b => b.CreatedAt >= blogFilterDto.StartDate.Value);
            //}

            //if (blogFilterDto.EndDate.HasValue)
            //{
            //    query = query.Where(b => b.CreatedAt <= blogFilterDto.EndDate.Value);
            //}

            // Phân trang
            query = query.Skip((pageIndex - 1) * limit).Take(limit);

            // Thực thi truy vấn và chuyển đổi sang DTO
            IEnumerable<Blogs> blogs = await query.ToListAsync();
            IEnumerable<BlogDto> blogDtos = _mapper.Map<IEnumerable<BlogDto>>(blogs);

            if (!blogDtos.Any())
            {
                return new PaginatedResult<BlogDto>
                {
                    Items = [],
                    TotalCount = 0,
                };
            }

            long totalCount = await _unitOfWork.GetCollection<Blogs>()
                .CountDocumentsAsync(b => (string.IsNullOrEmpty(blogFilterDto.SearchTerm) || b.Title.Contains(blogFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase)) &&
                                  (string.IsNullOrEmpty(blogFilterDto.Slug) || b.Slug == blogFilterDto.Slug) &&
                                  (blogFilterDto.Tags != null || (b.Tags != null)));

            return new PaginatedResult<BlogDto>
            {
                Items = blogDtos,
                TotalCount = totalCount,
            };
        }

        public async Task<IEnumerable<string>> GetTagsAsync()
        {
            // Return the result of the aggregation query
            return await _unitOfWork.GetCollection<Blogs>()
                .Aggregate()
                .Unwind<Blogs, BlogProjection>(b => b.Tags)
                .Project(b => b.Tags)
                .ToListAsync();
        }
    }
}
