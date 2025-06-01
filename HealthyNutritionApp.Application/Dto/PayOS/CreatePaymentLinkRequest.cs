using HealthyNutritionApp.Application.Dto.Order;

namespace HealthyNutritionApp.Application.Dto.PayOS
{
    public record CreatePaymentLinkRequest
    (
        OrderInformationRequest OrderInformation,
        string ReturnUrl,
        string CancelUrl
    );
}
