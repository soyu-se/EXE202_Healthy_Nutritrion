using AutoMapper;
using HealthyNutritionApp.Application.Dto.Order;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Order;
using HealthyNutritionApp.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Serilog;

namespace HealthyNutritionApp.Infrastructure.Services.Order
{
    public class OrderServices : IOrderServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
    }
}
