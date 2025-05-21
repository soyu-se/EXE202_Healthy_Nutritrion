using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Application.Models
{
    public class Payments
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string OrderId { get; set; }

        public string PaymentMethod { get; set; }

        public decimal Amount { get; set; }

        public string PaymentStatus { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TransactionId { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime PaidAt { get; set; }
    }
}
