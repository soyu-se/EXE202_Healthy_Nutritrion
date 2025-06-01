using HealthyNutritionApp.Application.Dto.PayOS;
using HealthyNutritionApp.Application.Interfaces.Order;
using HealthyNutritionApp.Application.ThirdPartyServices;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

namespace HealthyNutritionApp.Controllers.Payment
{
    [Route("api/v1/orders")]
    [ApiController]

    public class PaymentController : Controller
    {
        private readonly IOrderServices _orderServices;
        public PaymentController(IOrderServices payOSServices)
        {
            _orderServices = payOSServices;
        }

        /// <summary>
        /// Lấy thông tin Order (Bill) thông qua order code
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("get-order")]
        public async Task<IActionResult> GetOrderInformation(int orderId)
        {
            var result = await _orderServices.GetOrderDetails(orderId);
            return Ok(result);
        }

        /// <summary>
        /// Tạo liên kết thanh tóan payOS
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequest request)
        {
            var result = await _orderServices.GeneratePaymentLink(request);
            return Ok(result);
        }

        /// <summary>
        /// Sau khi thanh toán sẽ trr về webhook trên url, dùng nó để cập nhật thông tin đơn vào Db
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("update-order-payment")]
        public async Task<IActionResult> HandleWebhook([FromBody] WebhookType request)
        {
            await _orderServices.HandleWebhook(request);
            return Ok("Update successfully!");
        }

    }
}
