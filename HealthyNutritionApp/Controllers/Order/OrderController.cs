using HealthyNutritionApp.Application.Dto.Order;
using HealthyNutritionApp.Application.Interfaces.Order;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Order
{
    [Route("/api/v1/orders")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrderController : Controller
    {
        private readonly IOrderServices _orderServices;
        public OrderController(IOrderServices payOSServices)
        {
            _orderServices = payOSServices;
        }

        /// <summary>
        /// Lấy thông tin Order (Bill) thông qua order code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, User"), HttpGet("{code}")]
        public async Task<IActionResult> GetOrderInformation(int code)
        {
            var result = await _orderServices.GetOrderDetails(code);
            return Ok(result);
        }


        /// <summary>
        /// Lấy tất cả order
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin"), HttpGet()]
        public async Task<IActionResult> GetAllOrders([FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
        {
            var result = await _orderServices.GetOrderList(pageIndex, limit);
            return Ok(result);
        }

        /// <summary>
        /// Lấy tất cả đơn hàng của người dùng đã đăng nhập
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "User"), HttpGet("by-user")]
        public async Task<IActionResult> GetAllOrderByUser()
        {
            var result = await _orderServices.GetUserOrderList();
            return Ok(result);
        }

        [Authorize(Roles = "Admin"), HttpPut("update-status")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderStatusUpdateRequest request)
        {
            await _orderServices.UpdateOrderStatus(request);
            return Ok(new { message = "Order status updated successfully." });
        }
    }
}
