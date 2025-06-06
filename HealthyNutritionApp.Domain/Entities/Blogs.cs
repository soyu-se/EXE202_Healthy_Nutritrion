using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Domain.Entities
{
    public class Blogs
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        [BsonId]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Excerpt { get; set; }
        public string Slug { get; set; }
        public List<string> Tags { get; set; } = [];
        public List<string> Images { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
