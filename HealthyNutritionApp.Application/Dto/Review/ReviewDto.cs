using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Review
{
    public class ReviewDto : IMapFrom<Reviews>
    {
        public string Id { get; set; } // Unique identifier for the review
        public string UserId { get; set; } // ID of the user who submitted the review
        public string ProductId { get; set; } // ID of the product being reviewed
        public double Rating { get; set; } // Rating given by the user, typically between 1 and 5
        public string Comment { get; set; } // Text comment of the review
        public DateTime CreatedAt { get; set; } // Timestamp when the review was created
        public DateTime UpdatedAt { get; set; } // Timestamp when the review was last updated
    }
}
