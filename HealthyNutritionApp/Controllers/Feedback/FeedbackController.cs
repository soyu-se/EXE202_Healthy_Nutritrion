using HealthyNutritionApp.Application.Dto.Feedback;
using HealthyNutritionApp.Application.Interfaces.Feedback;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Feedback
{
    [Route("api/feedbacks")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class FeedbackController(IFeedbackService feedbackService) : ControllerBase
    {
        private readonly IFeedbackService _feedbackService = feedbackService;

        [Authorize(Roles = "Admin"), HttpGet]
        public async Task<IActionResult> GetFeedbacksAsync([FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
        {
            var result = await _feedbackService.GetFeedbacksAsync(pageIndex, limit);
            return Ok(new { message = "Feedbacks retrieved successfully", result });
        }

        [Authorize(Roles = "Admin"), HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackByIdAsync(string id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            return Ok(new { message = $"Feedback with ID {id} retrieved successfully", feedback });
        }

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> SubmitFeedbackAsync([FromBody] CreateFeedbackDto createFeedbackDto)
        {
            await _feedbackService.SubmitFeedbackAsync(createFeedbackDto);
            return Ok(new { message = "Feedback submitted successfully" });
        }

        [Authorize(Roles = "Admin"), HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedbackAsync(string id)
        {
            await _feedbackService.DeleteFeedbackAsync(id);
            return Ok(new { message = $"Feedback with ID {id} deleted successfully" });
        }
    }
}

