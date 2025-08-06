using System.Linq.Expressions;
using AutoMapper;
using CloudinaryDotNet.Actions;
using HealthyNutritionApp.Application.Dto.Order;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Order;
using HealthyNutritionApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Serilog;

namespace HealthyNutritionApp.Infrastructure.Services.Order
{
    public class OrderServices : IOrderServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderServices(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrderInformationResponse> GetOrderDetails(int orderCode)
        {
            Orders orders = await _unitOfWork.GetCollection<Orders>().Find(o => o.PayOSOrderCode == orderCode).FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Order is not exist!");

            var productIds = orders.Items.Select(i => i.ProductId).Distinct();

            var products = await _unitOfWork.GetCollection<Products>().Find(p => productIds.Contains(p.Id)).ToListAsync();

            Log.Information("The order with {OrderCode} is not exist in the system!", orderCode);
            //Map order vao order info response
            OrderInformationResponse response = _mapper.Map<OrderInformationResponse>(orders);

            foreach (var cartItem in response.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product != null && product.ImageUrls != null && product.ImageUrls.Any())
                {
                    cartItem.ProductImageUrl = product.ImageUrls.First();
                }
            }

            return response;
        }

        public async Task<PaginatedResult<OrderListResponse>> GetOrderList(string status, int pageIndex = 1, int limit = 10)
        {
            IQueryable<Orders> query = _unitOfWork.GetCollection<Orders>().AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            query = query.Skip((pageIndex - 1) * limit).Take(limit);

            IEnumerable<Orders> orders = await query.ToListAsync();

            // Lấy field CreatedAt từ Transaction

            IEnumerable<OrderListResponse> orderResponseList = _mapper.Map<IEnumerable<OrderListResponse>>(orders);
            // Map CreatedAt to CreatedAt in OrderListResponse
            foreach (OrderListResponse order in orderResponseList)
            {
                Transactions? transaction = await _unitOfWork.GetCollection<Transactions>().Find(t => t.OrderCode == order.OrderCode).FirstOrDefaultAsync();
                if (transaction != null)
                {
                    order.CreatedAt = transaction.CreatedAt;
                }
            }

            Expression<Func<Orders, bool>> filterExpression = o => string.IsNullOrEmpty(status) || o.Status == status;

            long totalCount = _unitOfWork.GetCollection<Orders>().CountDocuments(filterExpression);

            return new PaginatedResult<OrderListResponse>
            {
                Items = orderResponseList,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResult<OrderListResponse>> GetUserOrderList()
        {
            string userId = _httpContextAccessor.HttpContext.User.FindFirst("Id")?.Value
                            ?? throw new UnauthorizedCustomException("Your session is limit, you must login again to edit profile!");

            IQueryable<Orders> query = _unitOfWork.GetCollection<Orders>().AsQueryable();

            query = query.Where(o => o.UserId == userId);

            IEnumerable<Orders> orders = await query.ToListAsync();
            IEnumerable<OrderListResponse> orderResponseList = _mapper.Map<IEnumerable<OrderListResponse>>(orders);

            long totalCount = _unitOfWork.GetCollection<Orders>().CountDocuments(FilterDefinition<Orders>.Empty);

            return new PaginatedResult<OrderListResponse>
            {
                Items = orderResponseList,
                TotalCount = totalCount
            };
        }

        public async Task UpdateOrderStatus(OrderStatusUpdateRequest request)
        {
            Orders order = await _unitOfWork.GetCollection<Orders>()
                .Find(o => o.PayOSOrderCode == request.OrderCode)
                .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Order is not exist!");

            FilterDefinition<Orders> filter = Builders<Orders>.Filter.Eq(o => o.PayOSOrderCode, request.OrderCode);
            UpdateDefinition<Orders> update = Builders<Orders>.Update.Set(o => o.Status, request.Status);
            UpdateResult result = await _unitOfWork.GetCollection<Orders>().UpdateOneAsync(filter, update);
        }

    }
}
