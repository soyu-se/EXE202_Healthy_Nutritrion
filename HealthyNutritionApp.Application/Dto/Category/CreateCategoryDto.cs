namespace HealthyNutritionApp.Application.Dto.Category
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }

        // "goal" hoặc "product"
        public string Type { get; set; }

        public string Description { get; set; }
    }
}
