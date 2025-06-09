using HealthyNutritionApp.Application.Dto.Transaction;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Transaction;
using HealthyNutritionApp.Domain.Entities;
using MongoDB.Driver;

namespace HealthyNutritionApp.Infrastructure.Services.Transaction
{
    public class TransactionServices : ITransactionServices
    {

        private readonly IUnitOfWork _unitOfWork;
        public TransactionServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TransactionRevenueByDayResponse>> GetTransactionRevenueByDay(DateOnly fromDate, DateOnly toDate)
        {
            if(fromDate > toDate)
            {
                throw new BadRequestCustomException("From Date must be less than or equal to Final Date");
            }

            // lay danh sach giao dich trong khoang thoi gian tu fromDate den toDate
            var transactions = await _unitOfWork.GetCollection<Transactions>()
                .Find(t => t.CreatedAt.Date >= fromDate.ToDateTime(TimeOnly.MinValue) && t.CreatedAt.Date <= toDate.ToDateTime(TimeOnly.MaxValue))
                .ToListAsync();

            //nhom va tinh tong doanh thu theo ngay
            var revenueByDay = transactions
                .GroupBy(t => DateOnly.FromDateTime(t.CreatedAt))
                .Select(g => new TransactionRevenueByDayResponse
                {
                    Date = g.Key,
                    Amount = g.Sum(t => t.OrderAmount)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return revenueByDay;
        }
    }
}
