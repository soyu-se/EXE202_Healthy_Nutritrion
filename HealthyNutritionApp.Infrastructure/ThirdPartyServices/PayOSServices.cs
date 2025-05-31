using HealthyNutritionApp.Application.Dto.PayOS;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.ThirdPartyServices;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Infrastructure.Exceptions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Net.payOS;
using Net.payOS.Types;
using Serilog;

namespace HealthyNutritionApp.Infrastructure.ThirdPartyServices
{
    public class PayOSServices : IPayOSServices
    {
        private readonly IUnitOfWork _unitOfWork;

        #region Get Environment Variable for PayOS Arguments
        private static readonly string clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID") ?? throw new ExternalServiceCustomException("Pay OS client Id is not found!");
        private static readonly string apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY") ?? throw new ExternalServiceCustomException("Pay OS api key is not found!");
        private static readonly string checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY") ?? throw new ExternalServiceCustomException("Pay OS checksum key is not found!");
        #endregion

        private readonly PayOS _payOS = new PayOS(clientId, apiKey, checksumKey);

        public PayOSServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task HandleWebhook(WebhookType webhookType)
        {
            WebhookData data = _payOS.verifyPaymentWebhookData(webhookType);

            Orders order = await _unitOfWork.GetCollection<Orders>().Find(p => p.PayOSOrderCode == data.orderCode).FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Not found any order by this ID.");

            FilterDefinition<Orders> filter = Builders<Orders>.Filter.Eq(o => o.PayOSOrderCode, data.orderCode);
            UpdateDefinition<Orders> updateDefinition;
            //00 la thanh cong; 01 la that bai do sai tham so
            if (data.code == "00")
            {
                updateDefinition = Builders<Orders>.Update.Set(o => o.Status, "PAID");
                await _unitOfWork.GetCollection<Orders>().UpdateOneAsync(filter, updateDefinition);
                Log.Information("Order {OrderId} paid successfully at {TimeNow}", data.orderCode, DateTime.Now);
                return;
            }

            updateDefinition = Builders<Orders>.Update.Set(o => o.Status, "CANCELLED");
            await _unitOfWork.GetCollection<Orders>().UpdateOneAsync(filter, updateDefinition);
            Log.Error("Order {OrderId} is cancelled by causing error in payment progress!");

        }

        public async Task<CreatePaymentResult> CreatePaymentLink(CreatePaymentLinkRequest request)
        {
            //Lay ra thong tin Cart hop le
            Carts cart = _unitOfWork.GetCollection<Carts>().Find(c => c.Id == request.CartId).FirstOrDefault() ?? throw new NotFoundCustomException("Product cart is not found!");

            //Tao mot code moi cho Order
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            long expiredAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(15)).ToUnixTimeSeconds();

            List<ItemData> items = new List<ItemData>();

            foreach (CartItem cartItems in GetCartItems(cart.Items))
            {
                ItemData itemData = new ItemData(cartItems.ProductName, cartItems.Quantity, cartItems.PricePerUnit);
                items.Add(itemData);
            }


            //Lay tong amount tat ca san pham trong cart
            int cartAmount = await _unitOfWork.GetCollection<Carts>().AsQueryable()
                                                                     .Where(c => c.Id == request.CartId)
                                                                     .Select(c => c.Items.Sum(item => item.Quantity * item.PricePerUnit))
                                                                     .FirstOrDefaultAsync();
            Log.Information("total amount of cart is: {CartAmount}",cartAmount);

            PaymentData paymentData = new(
                    orderCode: orderCode,
                    amount: cartAmount,
                    description: "Healthy Nutrition Payment",
                    items: items,
                    cancelUrl: request.CancelUrl,
                    returnUrl: request.ReturnUrl,
                    expiredAt: expiredAt
                );

            CreatePaymentResult paymentResult = await _payOS.createPaymentLink(paymentData);
            return paymentResult;
        }

        public async Task<PaymentLinkInformation> GetOrder(int orderId)
        {
            return await _payOS.getPaymentLinkInformation(orderId);
        }

        public async Task<PaymentLinkInformation> CancelOrder(int payOSOrderCode, string? reason)
        {

            Log.Information("The order with code {OrderCode} is cancelled!");
            return await _payOS.cancelPaymentLink(payOSOrderCode, reason);
        }

        public async Task<string> ConfirmWebhookAsync(ConfirmWebhookRequest request)
        {
            return await _payOS.confirmWebhook(request.Webhook_Url);
        }



        private IEnumerable<CartItem> GetCartItems(List<CartItem> cartItems)
        {
            foreach (CartItem cartItem in cartItems)
            {
                yield return cartItem;
            }
        }
    }
}
