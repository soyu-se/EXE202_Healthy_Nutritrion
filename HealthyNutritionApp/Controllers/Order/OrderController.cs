using HealthyNutritionApp.Application.Dto.PayOS;
using HealthyNutritionApp.Application.ThirdPartyServices;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Order
{
    [Route("api/v1/orders")]
    [ApiController]

    public class OrderController : Controller
    {
        private readonly IPayOSServices _payOSServices;
        public OrderController(IPayOSServices payOSServices)
        {
            _payOSServices = payOSServices;
        }

        [HttpGet("get-order")]
        public async Task<IActionResult> GetOrderInformation(int orderId)
        {
            var result = await _payOSServices.GetOrder(orderId);
            return Ok(result);
        }


        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest request)
        {
            var result = await _payOSServices.CreatePaymentLink(request);
            return Ok(result);
        }

        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(ConfirmWebhookRequest request)
        {
            var result = await _payOSServices.ConfirmWebhookAsync(request);
            return Ok(result);
        }

    }
}
