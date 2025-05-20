using MongoDB.Bson;

namespace HealthyNutritionApp.Application.Models
{
    public class CartItem
    {
        public ObjectId ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
