
namespace HealthyNutritionApp.Application.Dto.Order
{
    public record OrderInformationRequest (List<CartItems> Items);

    public record CartItems(
        string ProductId,
        string ProductName,
        int Quantity,
        int PricePerUnit
    );
}
