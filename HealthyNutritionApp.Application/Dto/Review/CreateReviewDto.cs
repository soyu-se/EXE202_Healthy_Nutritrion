namespace HealthyNutritionApp.Application.Dto.Review
{
    public class CreateReviewDto
    {
        public string ProductId { get; set; } // The ID of the product being reviewed
        public string UserId { get; set; } // The ID of the user submitting the review
        public double Rating { get; set; } // The rating given by the user, typically between 1 and 5
        public string Comment { get; set; } // The text comment of the review
    }
}
