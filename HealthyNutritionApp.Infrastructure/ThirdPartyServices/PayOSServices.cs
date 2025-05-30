using HealthyNutritionApp.Application.Dto.PayOS;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.ThirdPartyService;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Infrastructure.Exceptions;
using MongoDB.Driver;
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
            //Tao mot code moi cho Order
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            ItemData itemData = new ItemData(request.ProductName, 1, request.Price);
            List<ItemData> items = new List<ItemData>();
            items.Add(itemData);
            PaymentData paymentData = new PaymentData(orderCode, request.Price, request.Description, items, request.CancelUrl, request.ReturnUrl);

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
    }
}
