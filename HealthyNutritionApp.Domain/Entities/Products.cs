using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HealthyNutritionApp.Domain.Entities
{
    public class Products
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public double Price { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> CategoryIds { get; set; }

        public string Brand { get; set; }

        public List<string> Tags { get; set; }

        public int StockQuantity { get; set; }

        public List<string> ImageUrls { get; set; }

        public NutritionFact NutritionFact { get; set; }
    }
}
