using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Application.Models
{
    public class Users
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.String)]
        public required string FullName { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string? Email { get; set; }

        [BsonRepresentation(BsonType.String)]
        public required string HashedPassword { get; set; }

        [BsonRepresentation(BsonType.String)]
        public required string Phone { get; set; }

        [BsonRepresentation(BsonType.String)]
        public required string Address { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime UpdatedAt { get; set; }
    }
}
