using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Product
{
    public class CreateProductDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public double Price { get; set; }

        public List<string> CategoryIds { get; set; }

        public string Brand { get; set; }

        public List<string> Tags { get; set; }

        public int StockQuantity { get; set; }

        public List<string> ImageUrls { get; set; }

        public NutritionFact NutritionFact { get; set; }
    }
}
