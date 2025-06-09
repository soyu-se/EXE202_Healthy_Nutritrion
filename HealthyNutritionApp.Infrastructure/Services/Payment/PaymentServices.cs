using AutoMapper;
using HealthyNutritionApp.Application.Dto.Order;
using HealthyNutritionApp.Application.Dto.Payment;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Payment;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Domain.Utils;
using HealthyNutritionApp.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Net.payOS;
using Net.payOS.Types;
using Serilog;

namespace HealthyNutritionApp.Infrastructure.Services.Payment
{
    public class PaymentServices : IPaymentServices
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        #region Get Environment Variable for PayOS Arguments
        private static readonly string clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID") ?? throw new ExternalServiceCustomException("Pay OS client Id is not found!");
        private static readonly string apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY") ?? throw new ExternalServiceCustomException("Pay OS api key is not found!");
        private static readonly string checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY") ?? throw new ExternalServiceCustomException("Pay OS checksum key is not found!");
        #endregion

        private readonly PayOS _payOs = new(clientId, apiKey, checksumKey);

        public PaymentServices(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<CreatePaymentResult> GeneratePaymentLink(CreatePaymentLinkRequest request)
        {
            //Tao mot code moi cho Order
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("fffffff"));
            long expiredAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(15)).ToUnixTimeSeconds();

            List<ItemData> items = new List<ItemData>();


            //Lay tat ca items trong cartItem vaf add vao List
            foreach (CartItems cartItems in GetCartItems(request.OrderInformation.Items))
            {
                ItemData itemData = new ItemData(cartItems.ProductName, cartItems.Quantity, cartItems.PricePerUnit);
                items.Add(itemData);
            }


            //Lay tong amount tat ca san pham trong cart
            int cartAmount = request.OrderInformation.Items.Sum(item => item.Quantity * item.PricePerUnit);

            Log.Information("total amount of cart is: {CartAmount}", cartAmount);

            //Dua cac tham so can thiet cho PayOs de thuc hien giao dich
            PaymentData paymentData = new(
                    orderCode: orderCode,
                    amount: cartAmount,
                    description: "Healthy Nutrition Payment",
                    items: items,
                    cancelUrl: request.CancelUrl,
                    returnUrl: request.ReturnUrl,
                    expiredAt: expiredAt
            );

            Orders order = _mapper.Map<OrderInformationRequest, Orders>(request.OrderInformation);

            order.PayOSOrderCode = orderCode;

            order.UserId = _contextAccessor.HttpContext?.User.FindFirst("Id")?.Value ?? throw new UnauthorizedCustomException("Your session is limit, please log in again to continue!");

            order.Status = "PENDING";

            await _unitOfWork.GetCollection<Orders>().InsertOneAsync(order);

            CreatePaymentResult paymentResult = await _payOs.createPaymentLink(paymentData);
            return paymentResult;
        }

        public async Task<PaymentLinkInformation> CancelPaymentLink(CancelPaymentLinkRequest request)
        {
            PaymentLinkInformation paymentLinkInformation = await _payOs.cancelPaymentLink(request.OrderCode, request.Reason);

            //Huy link -> dat lai trang thai order
            Orders order = await _unitOfWork.GetCollection<Orders>().Find(o => o.PayOSOrderCode == request.OrderCode).FirstOrDefaultAsync() 
                                 ?? throw new NotFoundCustomException("Cannot find the order with Order code!") ;

            UpdateDefinition<Orders> updateDefinition = Builders<Orders>.Update.Set(o => o.Status, "CANCELLED");

            await _unitOfWork.GetCollection<Orders>().UpdateOneAsync(o => o.PayOSOrderCode == request.OrderCode, updateDefinition);

            return paymentLinkInformation;
        }


        public async Task HandleWebhook(WebhookType webhookType)
        {
            WebhookData data = _payOs.verifyPaymentWebhookData(webhookType);

            Log.Information("INFORMATION FROM PAYOS: {OrderCode} \n {Amount} \n {Description}\n {a}\n {b}\n {d}\n {c}\n {e}\n {f}\n {g}\n {h}\n {i}\n {j}\n {Description}\n {k}\n {l}", webhookType.code, webhookType.data.amount, webhookType.data.description, webhookType.data.accountNumber, webhookType.data.reference, webhookType.data.transactionDateTime, webhookType.data.currency, webhookType.data.paymentLinkId, webhookType.data.code, webhookType.data.desc, webhookType.data.counterAccountBankId, webhookType.data.counterAccountBankName, webhookType.data.counterAccountName, webhookType.data.counterAccountNumber, webhookType.data.virtualAccountName , webhookType.data.virtualAccountNumber);

            //Orders order = await _unitOfWork.GetCollection<Orders>().Find(p => p.PayOSOrderCode == data.orderCode).FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Not found any order by this ID.");

            UpdateDefinition<Orders> updateDefinition;
            //00 la thanh cong; 01 la that bai do sai tham so
            if (webhookType.success)
            {
                updateDefinition = Builders<Orders>.Update.Set(o => o.Status, "PAID")
                                                          .Set(o => o.TotalAmount, data.amount);
                await _unitOfWork.GetCollection<Orders>().UpdateOneAsync(o => o.PayOSOrderCode == data.orderCode, updateDefinition);
                Log.Information("Order {OrderId} paid successfully at {TimeNow}", data.orderCode, DateTime.Now);

                Transactions transaction = new Transactions()
                {
                    OrderAmount = data.amount,
                    PaymentMethod = "Pay OS",
                    BankAccountNumber = data.accountNumber,
                    OrderCode = data.orderCode,
                    PaymentStatus = "PAID",
                    CreatedAt = DateTime.Parse(data.transactionDateTime)
                };
                await _unitOfWork.GetCollection<Transactions>().InsertOneAsync(transaction);
                return;
            }

            updateDefinition = Builders<Orders>.Update.Set(o => o.Status, "FAILED")
                                                      .Set(o => o.TotalAmount, data.amount);
            await _unitOfWork.GetCollection<Orders>().UpdateOneAsync(o => o.PayOSOrderCode == data.orderCode, updateDefinition);
            Log.Error("Order {OrderId} is cancelled by causing error in payment progress!");
            return;
        }

        public async Task<string> ConfirmWebhook(String url)
        {
            string result = "";
            try
            {
                result = await _payOs.confirmWebhook(url);
            }
            catch (Exception e)
            {
                Log.Error("Error occurs when confirmming url:", e.Message);
                throw new BadRequestCustomException("Cannot confirm this url to receive webhook!");
            }
            return result;
        }

        private static IEnumerable<CartItems> GetCartItems(List<CartItems> cartItems)
        {
            foreach (CartItems cartItem in cartItems)
            {
                yield return cartItem;
            }
        }
    }
}
