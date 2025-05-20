using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HealthyNutritionApp.Application.Models
{
    public class Products
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.String)]
        public required string Name { get; set; }

        [BsonRepresentation(BsonType.String)]
        public required string Description { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public required double Price { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ObjectId CategoryId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string Brand { get; set; }

        public List<string> Tags { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public int StockQuantity { get; set; }

        [BsonRepresentation(BsonType.Array)]
        public List<string> ImageUrls { get; set; }

        public NutritionFact NutritionFact { get; set; }

        [BsonRepresentation(BsonType.Boolean)]
        public bool IsFeatured { get; set; }
    }
}
