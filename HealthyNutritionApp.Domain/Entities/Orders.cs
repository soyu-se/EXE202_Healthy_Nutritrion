using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Domain.Entities
{
    public class Orders
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public List<OrderItems> Items { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public string ShippingAddress { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime UpdatedAt { get; set; }
    }
}
