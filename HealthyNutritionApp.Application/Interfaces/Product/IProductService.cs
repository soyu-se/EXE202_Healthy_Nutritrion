using HealthyNutritionApp.Application.Dto.Product;

namespace HealthyNutritionApp.Application.Interfaces.Product
{
    public interface IProductService
    {
        Task CreateProductAsync(CreateProductDto productDto);
        Task DeleteProductAsync(string id);
        Task<ProductDto> GetProductByIdAsync(string id);
        Task<IEnumerable<ProductDto>> GetProductsAsync(ProductFilterDto productFilterDto, int offset = 1, int limit = 10);
        Task UpdateProductAsync(string id, UpdateProductDto updateProductDto);
    }
}
