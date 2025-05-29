using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Domain.Entities
{
    public class Feedback
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
