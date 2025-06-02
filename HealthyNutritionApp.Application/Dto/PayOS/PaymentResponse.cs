namespace HealthyNutritionApp.Application.Dto.Payment
{
    public record PaymentResponse
    (
        int Error,
        String Message,
        object? Data
    );
}
