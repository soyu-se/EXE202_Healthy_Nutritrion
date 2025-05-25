using HealthyNutritionApp.Application.Dto.Category;
using HealthyNutritionApp.Application.Interfaces.Category;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Category
{
    [Route("api/categories")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        private readonly ICategoryService _categoryService = categoryService;

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> GetCategoriesAsync([FromQuery] CategoryFilterDto categoryFilterDto, [FromQuery] int offset = 1, [FromQuery] int limit = 10)
        {
            var result = await _categoryService.GetCategoriesAsync(categoryFilterDto, offset, limit);
            return Ok(new { message = "Categories retrieved successfully", result });
        }

        [AllowAnonymous, HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryByIdAsync(string id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found" });
            }
            return Ok(new { message = $"Category with ID {id} retrieved successfully", category });
        }

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> CreateCategoryAsync([FromBody] CreateCategoryDto categoryDto)
        {
            await _categoryService.CreateCategoryAsync(categoryDto);
            return Ok(new { message = "Category created successfully" });
        }

        [AllowAnonymous, HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoryAsync(string id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
            return Ok(new { message = "Category updated successfully" });
        }

        [AllowAnonymous, HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoryAsync(string id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new { message = $"Category with ID {id} deleted successfully" });
        }
    }
}
