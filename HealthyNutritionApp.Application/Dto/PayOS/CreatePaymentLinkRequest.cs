namespace HealthyNutritionApp.Application.Dto.PayOS
{
    public record CreatePaymentLinkRequest
    (
        string ProductName,
        string Description,
        int Price,
        string ReturnUrl,
        string CancelUrl
    );
}
