using HealthyNutritionApp.Application.Dto.Payment;
using HealthyNutritionApp.Application.Interfaces.Payment;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

namespace HealthyNutritionApp.Controllers.Payment
{
    [Route("api/v1/orders")]
    [ApiController]

    public class PaymentController : Controller
    {
        private readonly IPaymentServices _paymentServices;
        public PaymentController(IPaymentServices paymentServices)
        {
            _paymentServices = paymentServices;
        }

        /// <summary>
        /// Tạo liên kết thanh tóan payOS
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequest request)
        {
            var result = await _paymentServices.GeneratePaymentLink(request);
            return Ok(result);
        }

        /// <summary>
        ///  API này Payment sẽ gọi
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("handle-webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] WebhookType request)
        {
            await _paymentServices.HandleWebhook(request);
            return Ok("Update successfully!");
        }

        /// <summary>
        /// Xác nhận link nhận webhook với payOs
        /// </summary>
        /// <returns></returns>
        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(string url)
        {
            await _paymentServices.ConfirmWebhook(url);
            return Ok("Confirm webhook successfully!");
        }

        
        [HttpPatch("cancel-payment")]
        public async Task<IActionResult> CancelPaymentLink([FromBody] CancelPaymentLinkRequest request)
        {
            var result = await _paymentServices.CancelPaymentLink(request);
            return Ok(result);
        }
    }
}
