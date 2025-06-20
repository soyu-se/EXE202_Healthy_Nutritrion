namespace HealthyNutritionApp.Application.Projections.Dto.Blog
{
    public record BlogProjection
    {
        public string Tags { get; init; } = default!;
        //public string Title { get; init; } = default!;
    };
}
