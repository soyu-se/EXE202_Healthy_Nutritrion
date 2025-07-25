﻿using HealthyNutritionApp.Application.Dto.Product;
using HealthyNutritionApp.Application.Interfaces.Product;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthyNutritionApp.Controllers.Product
{
    [Route("api/v1/products")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class ProductController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> GetProductsAsync([FromQuery] ProductFilterDto productFilterDto, [FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
        {
            var result = await _productService.GetProductsAsync(productFilterDto, pageIndex, limit);
            return Ok(new { message = "Product retrieved successfully", result });
        }

        [AllowAnonymous, HttpGet("{id}")]
        public async Task<IActionResult> GetProductByIdAsync(string id)
        {
            // Logic to get product by ID
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(new { message = $"Product with ID {id} retrieved successfully", product });
        }

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> CreateProductAsync(CreateProductDto productDto)
        {
            // Logic to create a new product
            await _productService.CreateProductAsync(productDto);
            return Ok(new { message = "Product created successfully" });
        }

        [AllowAnonymous, HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductAsync(string id, UpdateProductDto updateProductDto)
        {
            // Logic to update an existing product
            await _productService.UpdateProductAsync(id, updateProductDto);
            return Ok(new { message = "Product updated successfully" });
        }

        [AllowAnonymous, HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductAsync(string id)
        {
            // Logic to delete a product by ID
            await _productService.DeleteProductAsync(id);
            return Ok(new { message = $"Product with ID {id} deleted successfully" });
        }

        [AllowAnonymous, HttpGet("with-category/{id}")]
        public async Task<IActionResult> GetProductWithCategoryName(string id)
        {
            // Logic to get product with category name
            var product = await _productService.GetProductWithCategoryName(id);
            return Ok(new { message = $"Product with ID {id} and category name retrieved successfully", product });
        }
    }
}
