using HealthyNutritionApp.Application.Dto.Blog;
using HealthyNutritionApp.Application.Interfaces.Blog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Blog
{
    [Route("api/v1/blogs")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class BlogController(IBlogService blogService) : ControllerBase
    {
        private readonly IBlogService _blogService = blogService;

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> GetBlogsAsync([FromQuery] BlogFilterDto blogFilterDto, [FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
        {
            var result = await _blogService.GetBlogsAsync(blogFilterDto, pageIndex, limit);
            return Ok(new { message = "Blogs retrieved successfully", result });
        }

        [AllowAnonymous, HttpGet("{id}")]
        public async Task<IActionResult> GetBlogByIdAsync(string id)
        {
            var blog = await _blogService.GetBlogByIdAsync(id);
            return Ok(new { message = $"Blog with ID {id} retrieved successfully", blog });
        }

        [AllowAnonymous, HttpGet("test/slug")]
        public IActionResult GetBlogBySlugAsync(string slug)
        {
            var blog = _blogService.CreateSlug(slug);
            return Ok(new { message = $"Blog with slug '{slug}' retrieved successfully", blog });
        }

        [AllowAnonymous, HttpGet("tags")]
        public async Task<IActionResult> GetTagsAsync()
        {
            var tags = await _blogService.GetTagsAsync();
            return Ok(new { message = "Blogs retrieved successfully", tags });
        }

        [Authorize(Roles = "Admin"), HttpPost]
        public async Task<IActionResult> CreateBlogAsync(CreateBlogDto blogDto)
        {
            await _blogService.CreateBlogAsync(blogDto);
            return Ok(new { message = "Blog created successfully" });
        }

        [Authorize(Roles = "Admin"), HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlogAsync(string id, UpdateBlogDto updateBlogDto)
        {
            await _blogService.UpdateBlogAsync(id, updateBlogDto);
            return Ok(new { message = "Blog updated successfully" });
        }

        [Authorize(Roles = "Admin"), HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogAsync(string id)
        {
            await _blogService.DeleteBlogAsync(id);
            return Ok(new { message = $"Blog with ID {id} deleted successfully" });
        }
    }
}
