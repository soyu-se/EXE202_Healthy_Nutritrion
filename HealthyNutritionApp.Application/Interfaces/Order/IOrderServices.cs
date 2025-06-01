using HealthyNutritionApp.Application.Dto.Order;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Dto.PayOS;
using Net.payOS.Types;

namespace HealthyNutritionApp.Application.Interfaces.Order
{
    public interface IOrderServices
    {
        Task<CreatePaymentResult> GeneratePaymentLink(CreatePaymentLinkRequest request);
        Task<OrderInformationResponse> GetOrderDetails(int orderCode);
        Task<PaginatedResult<OrderListResponse>> GetOrderList(int pageIndex = 1, int limit = 10);
        Task HandleWebhook(WebhookType webhookType);
    }
}
