using HealthyNutritionApp.Application.Interfaces.Account;
using HealthyNutritionApp.Application.Interfaces.Feedback;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Dashboard
{
    [Route("api/v1/dashboards")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class DashboardController(IAccountService accountService, IFeedbackService feedbackService) : ControllerBase
    {
        private readonly IAccountService _accountService = accountService;
        private readonly IFeedbackService _feedbackService = feedbackService;

        [Authorize(Roles = "Admin"), HttpGet]
        public async Task<IActionResult> GetDashboardDataAsync()
        {
            // Lấy tổng số lượng người dùng
            long totalCountUsers = await _accountService.GetTotalCountUsersAsync();

            // Lấy tổng số lượng phản hồi
            long totalCountFeedbacks = await _feedbackService.GetTotalCountFeedbackAsync();

            long totalCountTransactions = 0;

            long totalCountRevenues = 0;

            // Trả về dữ liệu dashboard
            return Ok(new
            {
                TotalCountUsers = totalCountUsers,
                TotalCountFeedbacks = totalCountFeedbacks,
                TotalCountTransactions = totalCountTransactions,
                TotalCountRevenues = totalCountRevenues
            });
        }
    }
}
