namespace HealthyNutritionApp.Application.Dto.Category
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; } // Tên danh mục mới
        public string Type { get; set; } // Loại danh mục mới (ví dụ: "goal", "product")
        public string Description { get; set; } // Mô tả danh mục mới
    }
}
