using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Application.Models
{
    public class Payments
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId OrderId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string PaymentMethod { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Amount { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string PaymentStatus { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string TransactionId { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime PaidAt { get; set; }
    }
}
