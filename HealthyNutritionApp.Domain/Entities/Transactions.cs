
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Domain.Entities
{
    public class Transactions
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public long OrderCode { get; set; }
        public decimal OrderAmount { get; set; }

        public string BankAccountNumber { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }
}
