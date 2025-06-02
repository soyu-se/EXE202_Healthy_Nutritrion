namespace HealthyNutritionApp.Application.Dto.Payment
{
    public record CancelPaymentLinkRequest
    (
        long OrderCode,
        string? Reason
    );
}
