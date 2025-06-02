using HealthyNutritionApp.Application.Dto.Order;
using HealthyNutritionApp.Application.Dto.PaginatedResult;

namespace HealthyNutritionApp.Application.Interfaces.Order
{
    public interface IOrderServices
    {
        Task<OrderInformationResponse> GetOrderDetails(int orderCode);
        Task<PaginatedResult<OrderListResponse>> GetOrderList(int pageIndex = 1, int limit = 10);
    }
}
