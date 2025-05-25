using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Application.Dto.Review
{
    public class ReviewFilterDto
    {
        public string Id { get; set; }

        public string ProductId { get; set; }

        public string UserId { get; set; }

        public double Rating { get; set; }

        public string Comment { get; set; }
    }
}
