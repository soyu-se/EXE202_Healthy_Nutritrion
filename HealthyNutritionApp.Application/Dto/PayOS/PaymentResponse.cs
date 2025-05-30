namespace HealthyNutritionApp.Application.Dto.PayOS
{
    public record PaymentResponse
    (
        int Error,
        String Message,
        object? Data
    );
}
