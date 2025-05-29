using AutoMapper;
using HealthyNutritionApp.Application.Dto.Category;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Category;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Domain.Utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HealthyNutritionApp.Infrastructure.Implements.Category
{
    public class CategoryService(IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        // This class is currently empty, but it can be implemented later
        // to handle category-related operations such as creating, updating,
        // deleting, and retrieving categories.

        // Example methods could include:
        // - CreateCategoryAsync
        // - UpdateCategoryAsync
        // - DeleteCategoryAsync
        // - GetCategoryByIdAsync
        // - GetAllCategoriesAsync

        // These methods would typically interact with a database or an external service
        // to manage category data.
        // For now, this class serves as a placeholder for future category management functionality.

        public async Task<PaginatedResult<CategoryDto>> GetCategoriesAsync(CategoryFilterDto categoryFilterDto, int offset = 1, int limit = 10)
        {
            // Phân trang và lấy tất cả danh mục
            IQueryable<Categories> query = _unitOfWork.GetCollection<Categories>().AsQueryable();

            // Kiểm tra nếu có bộ lọc tìm kiếm
            if (!string.IsNullOrEmpty(categoryFilterDto.Name))
            {
                query = query.Where(c => c.Name.Contains(categoryFilterDto.Name, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrEmpty(categoryFilterDto.Type))
            {
                query = query.Where(c => c.Type.Equals(categoryFilterDto.Type, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrEmpty(categoryFilterDto.Description))
            {
                query = query.Where(c => c.Description.Contains(categoryFilterDto.Description, StringComparison.CurrentCultureIgnoreCase));
            }

            // Phân trang
            query = query.Skip((offset - 1) * limit).Take(limit);

            long totalCount = await _unitOfWork.GetCollection<Categories>()
                .CountDocumentsAsync(c => string.IsNullOrEmpty(categoryFilterDto.Name) || c.Name.Contains(categoryFilterDto.Name, StringComparison.CurrentCultureIgnoreCase) &&
                                          (string.IsNullOrEmpty(categoryFilterDto.Type) || c.Type.Equals(categoryFilterDto.Type, StringComparison.CurrentCultureIgnoreCase)) &&
                                          (string.IsNullOrEmpty(categoryFilterDto.Description) || c.Description.Contains(categoryFilterDto.Description, StringComparison.CurrentCultureIgnoreCase)));

            // Thực hiện truy vấn và chuyển đổi kết quả sang danh sách CategoryDto
            IEnumerable<Categories> categories = await query.ToListAsync();

            IEnumerable<CategoryDto> categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            return new PaginatedResult<CategoryDto>
            {
                Items = categoriesDto,
                TotalCount = totalCount,
            };
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(string id)
        {
            // Lấy danh mục theo Id
            Categories category = await _unitOfWork.GetCollection<Categories>()
                .Find(c => c.Id == id)
                .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Category Not Found");
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            // Implementation for creating a category
            Categories category = new()
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                Type = categoryDto.Type,
                CreateAt = TimeControl.GetUtcPlus7Time(),
            };

            await _unitOfWork.GetCollection<Categories>().InsertOneAsync(category);
        }

        public async Task UpdateCategoryAsync(string id, UpdateCategoryDto categoryDto)
        {
            // Implementation for updating a category
            Categories category = await _unitOfWork.GetCollection<Categories>()
                .Find(c => c.Id == id)
                .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Category Not Found");

            UpdateDefinition<Categories> update = Builders<Categories>.Update
                .Set(c => c.Name, categoryDto.Name)
                .Set(c => c.Type, categoryDto.Type)
                .Set(c => c.Description, categoryDto.Description);

            await _unitOfWork.GetCollection<Categories>()
                .UpdateOneAsync(c => c.Id == id, update);
        }

        public async Task DeleteCategoryAsync(string id)
        {
            // Implementation for deleting a category
            DeleteResult result = await _unitOfWork.GetCollection<Categories>()
                .DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                throw new NotFoundCustomException("Category Not Found");
            }
        }
    }
}
