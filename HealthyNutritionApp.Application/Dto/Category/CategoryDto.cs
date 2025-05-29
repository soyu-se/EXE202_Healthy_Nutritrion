using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Category
{
    public class CategoryDto : IMapFrom<Categories>
    {
        public string Id { get; set; } // Mã định danh duy nhất của danh mục
        public string Name { get; set; } // Tên danh mục
        public string Type { get; set; } // Loại danh mục (ví dụ: "goal", "product")
        public string Description { get; set; } // Mô tả danh mục
    }
}
