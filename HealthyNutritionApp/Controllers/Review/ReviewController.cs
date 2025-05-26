using HealthyNutritionApp.Application.Dto.Review;
using HealthyNutritionApp.Application.Interfaces.Review;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Review
{
    [Route("api/v1/reviews")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
        private readonly IReviewService _reviewService = reviewService;

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> GetReviewsAsync([FromQuery] ReviewFilterDto reviewFilterDto, [FromQuery] int offset = 1, [FromQuery] int limit = 10)
        {
            var result = await _reviewService.GetReviewsAsync(reviewFilterDto, offset, limit);
            return Ok(new { message = "Reviews retrieved successfully", result });
        }

        [AllowAnonymous, HttpGet("{id}")]
        public async Task<IActionResult> GetReviewByIdAsync(string id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
            {
                return NotFound(new { message = $"Review with ID {id} not found" });
            }
            return Ok(new { message = $"Review with ID {id} retrieved successfully", review });
        }

        [HttpPost]
        public async Task<IActionResult> CreateReviewAsync([FromBody] CreateReviewDto reviewDto)
        {
            await _reviewService.PostReviewAsync(reviewDto);
            return Ok(new { message = "Review created successfully" });
        }

        [AllowAnonymous, HttpPut("{id}")]
        public async Task<IActionResult> UpdateReviewAsync(string id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            await _reviewService.UpdateReviewAsync(id, updateReviewDto);
            return Ok(new { message = "Review updated successfully" });
        }

        [AllowAnonymous, HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReviewAsync(string id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return Ok(new { message = $"Review with ID {id} deleted successfully" });
        }
    }
}
