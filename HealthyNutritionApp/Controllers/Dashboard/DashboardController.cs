using HealthyNutritionApp.Application.Interfaces.Account;
using HealthyNutritionApp.Application.Interfaces.Feedback;
using HealthyNutritionApp.Application.Interfaces.Order;
using HealthyNutritionApp.Application.Interfaces.Transaction;
using HealthyNutritionApp.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Dashboard
{
    [Route("api/v1/dashboards")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    [Authorize(Roles = "Admin")]
    public class DashboardController(IAccountService accountService, IFeedbackService feedbackService, ITransactionServices transactionService) : ControllerBase
    {
        private readonly IAccountService _accountService = accountService;
        private readonly IFeedbackService _feedbackService = feedbackService;
        private readonly ITransactionServices _transactionServices = transactionService;

        [HttpGet]
        public async Task<IActionResult> GetDashboardDataAsync()
        {
            // Lấy tổng số lượng người dùng
            long totalCountUsers = await _accountService.GetTotalCountUsersAsync();

            // Lấy tổng số lượng phản hồi
            long totalCountFeedbacks = await _feedbackService.GetTotalCountFeedbackAsync();

            var transactionDto = await _transactionServices.GetTransactionsAsync();

            long totalCountTransactions = transactionDto.TotalCountTransactions;

            decimal totalCountRevenues = transactionDto.TotalCountRevenues;

            // Trả về dữ liệu dashboard
            return Ok(new
            {
                TotalCountUsers = totalCountUsers,
                TotalCountFeedbacks = totalCountFeedbacks,
                TotalCountTransactions = totalCountTransactions,
                TotalCountRevenues = totalCountRevenues
            });
        }

        [HttpGet("transactions-by-day")]
        public async Task<IActionResult> GetTransactionAmountByDays(DateOnly fromDate, DateOnly toDate)
        {
            var orderList = await _transactionServices.GetTransactionRevenueByDay(fromDate, toDate);
            return Ok(orderList);
        }

        [HttpGet("account-register-counting")]
        public async Task<IActionResult> GetAccountRegisterCounting(DateOnly fromDate, DateOnly toDate)
        {
            var accountRegisterCounting = await _accountService.AccountRegisterCountingResponses(fromDate, toDate);
            return Ok(accountRegisterCounting);
        }
    }
}
