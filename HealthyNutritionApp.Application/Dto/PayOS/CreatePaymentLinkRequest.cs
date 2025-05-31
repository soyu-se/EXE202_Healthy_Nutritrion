namespace HealthyNutritionApp.Application.Dto.PayOS
{
    public record CreatePaymentLinkRequest
    (
        string CartId,
        string ReturnUrl,
        string CancelUrl
    );
}
