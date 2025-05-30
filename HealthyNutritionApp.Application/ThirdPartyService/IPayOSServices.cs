using Net.payOS.Types;
using HealthyNutritionApp.Application.Dto.PayOS;

namespace HealthyNutritionApp.Application.ThirdPartyService
{
    public interface IPayOSServices
    {
        Task<PaymentLinkInformation> CancelOrder(int payOSOrderCode, string? reason);
        Task<string> ConfirmWebhookAsync(ConfirmWebhookRequest request);
        Task<CreatePaymentResult> CreatePaymentLink(CreatePaymentLinkRequest request);
        Task<PaymentLinkInformation> GetOrder(int orderId);
        Task HandleWebhook(WebhookType webhookType);
    }
}
