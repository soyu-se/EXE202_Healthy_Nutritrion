using HealthyNutritionApp.Application.Dto.Payment;
using Net.payOS.Types;

namespace HealthyNutritionApp.Application.Interfaces.Payment
{
    public interface IPaymentServices
    {
        Task<PaymentLinkInformation> CancelPaymentLink(CancelPaymentLinkRequest request);
        Task<string> ConfirmWebhook(string url);
        Task<CreatePaymentResult> GeneratePaymentLink(CreatePaymentLinkRequest request);
        Task HandleWebhook(WebhookType webhookType);

    }
}
