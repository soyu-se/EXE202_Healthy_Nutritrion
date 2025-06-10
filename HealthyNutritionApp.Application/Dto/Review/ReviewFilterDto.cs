namespace HealthyNutritionApp.Application.Dto.Review
{
    public class ReviewFilterDto
    {
        public string? ProductId { get; set; }

        public string? UserId { get; set; }

        public double Rating { get; set; }

        public string? Comment { get; set; }
    }
}
