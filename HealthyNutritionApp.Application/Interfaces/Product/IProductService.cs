﻿using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Dto.Product;

namespace HealthyNutritionApp.Application.Interfaces.Product
{
    public interface IProductService
    {
        Task CreateProductAsync(CreateProductDto productDto);
        Task DeleteProductAsync(string id);
        Task<ProductDto> GetProductByIdAsync(string id);
        Task<PaginatedResult<ProductDto>> GetProductsAsync(ProductFilterDto productFilterDto, int pageIndex = 1, int limit = 10);
        Task UpdateProductAsync(string id, UpdateProductDto updateProductDto);
    }
}
