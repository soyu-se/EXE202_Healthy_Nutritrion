namespace HealthyNutritionApp.Application.Dto.Review
{
    public class UpdateReviewDto
    {
        public double Rating { get; set; } // Rating given by the user, typically between 1 and 5
        public string Comment { get; set; } // Text comment of the review
    }
}
