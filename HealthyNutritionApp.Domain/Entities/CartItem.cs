using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Domain.Entities
{
    public class CartItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public int PricePerUnit { get; set; }
    }
}
