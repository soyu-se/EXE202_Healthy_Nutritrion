namespace HealthyNutritionApp.Application.Projections.Blog
{
    public record BlogProjection
    {
        public string Tags { get; init; } = default!;
        //public string Title { get; init; } = default!;
    };
}
