using HealthyNutritionApp.Application.Dto.Transaction;

namespace HealthyNutritionApp.Application.Interfaces.Transaction
{
    public interface ITransactionServices
    {
        Task<IEnumerable<TransactionRevenueByDayResponse>> GetTransactionRevenueByDay(DateOnly fromDate, DateOnly toDate);
        Task<TransactionDto> GetTransactionsAsync();
    }
}
