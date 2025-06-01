using AutoMapper;
using HealthyNutritionApp.Application.Dto.Order;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Dto.PayOS;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Order;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Infrastructure.Exceptions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Net.payOS;
using Net.payOS.Types;
using Serilog;

namespace HealthyNutritionApp.Infrastructure.Services.Order
{
    public class OrderServices : IOrderServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #region Get Environment Variable for PayOS Arguments
        private static readonly string clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID") ?? throw new ExternalServiceCustomException("Pay OS client Id is not found!");
        private static readonly string apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY") ?? throw new ExternalServiceCustomException("Pay OS api key is not found!");
        private static readonly string checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY") ?? throw new ExternalServiceCustomException("Pay OS checksum key is not found!");
        #endregion

        private readonly PayOS _payOs = new(clientId, apiKey, checksumKey);

        public OrderServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CreatePaymentResult> GeneratePaymentLink(CreatePaymentLinkRequest request)
        {
            //Tao mot code moi cho Order
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
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

            PaymentData paymentData = new(
                    orderCode: orderCode,
                    amount: cartAmount,
                    description: "Healthy Nutrition Payment",
                    items: items,
                    cancelUrl: request.CancelUrl,
                    returnUrl: request.ReturnUrl,
                    expiredAt: expiredAt
            );

            CreatePaymentResult paymentResult = await _payOs.createPaymentLink(paymentData);
            return paymentResult;
        }

        public async Task HandleWebhook(WebhookType webhookType)
        {
            WebhookData data = _payOs.verifyPaymentWebhookData(webhookType);

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

        public async Task<OrderInformationResponse> GetOrderDetails(int orderCode)
        {
            Orders order = await _unitOfWork.GetCollection<Orders>().Find(o => o.PayOSOrderCode == orderCode).FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Order is not exist!");

            Log.Information("The order with {OrderCode} is not exist in the system!", orderCode);
            //Map order vao order info response
            OrderInformationResponse response = _mapper.Map<OrderInformationResponse>(order);

            return response;
        }

        public async Task<PaginatedResult<OrderListResponse>> GetOrderList(int pageIndex = 1, int limit = 10)
        {
            IQueryable<Orders> query = _unitOfWork.GetCollection<Orders>().AsQueryable();

            query = query.Where(o => o.Status == "PAID");

            query = query.Skip((pageIndex - 1) * limit).Take(limit);

            IEnumerable<Orders> orders = await query.ToListAsync();
            IEnumerable<OrderListResponse> orderResponseList = _mapper.Map<IEnumerable<OrderListResponse>>(orders);

            long totalCount = await _unitOfWork.GetCollection<Orders>().CountDocumentsAsync(o => o.Status == "PAID");

            return new PaginatedResult<OrderListResponse>
            {
                Items = orderResponseList,
                TotalCount = totalCount
            };
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
