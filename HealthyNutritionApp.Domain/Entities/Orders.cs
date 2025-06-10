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

        public int PayOSOrderCode { get; set; }

        public List<OrderItems> Items { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }
    }

    public class OrderItems
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public double Weight { get; set; }

        public int Quantity { get; set; }

        public decimal PricePerKilogram { get; set; }
    }
}