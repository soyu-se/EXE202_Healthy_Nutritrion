using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HealthyNutritionApp.Application.Models
{
    public class Products
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public required string Name { get; set; }

        public required string Description { get; set; }

        public required double Price { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> CategoryIds { get; set; }

        public string Brand { get; set; }

        public List<string> Tags { get; set; }

        public int StockQuantity { get; set; }

        public List<string> ImageUrls { get; set; }

        public NutritionFact NutritionFact { get; set; }

        public bool IsFeatured { get; set; }
    }
}
