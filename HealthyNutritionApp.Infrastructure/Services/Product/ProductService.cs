using AutoMapper;
using CloudinaryDotNet.Actions;
using HealthyNutritionApp.Application.Dto;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Dto.Product;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Product;
using HealthyNutritionApp.Application.ThirdPartyServices.Cloudinary;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Domain.Enums;
using HealthyNutritionApp.Domain.Utils;
using HealthyNutritionApp.Infrastructure.ThirdPartyServices.Cloudinaries;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace HealthyNutritionApp.Infrastructure.Services.Product
{
    public class ProductService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService) : IProductService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICloudinaryService _cloudinaryService = cloudinaryService;

        public async Task<PaginatedResult<ProductDto>> GetProductsAsync(ProductFilterDto productFilterDto, int pageIndex = 1, int limit = 10)
        {
            // Bắt đầu với truy vấn cơ bản
            IQueryable<Products> query = _unitOfWork.GetCollection<Products>().AsQueryable();

            // Kiểm tra nếu có bộ lọc tìm kiếm
            if (!string.IsNullOrEmpty(productFilterDto.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(productFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                                         p.Description.Contains(productFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase));
            }

            // Lọc theo danh mục
            if (productFilterDto.CategoryIds != null && productFilterDto.CategoryIds.Count > 0)
            {
                query = query.Where(p => p.CategoryIds != null && p.CategoryIds.Any(c => productFilterDto.CategoryIds.Contains(c)));
            }

            // Lọc theo thương hiệu
            if (!string.IsNullOrEmpty(productFilterDto.Brand))
            {
                query = query.Where(p => p.Brand.Contains(productFilterDto.Brand, StringComparison.CurrentCultureIgnoreCase));
            }

            // Lọc theo thẻ
            if (productFilterDto.Tags != null && productFilterDto.Tags.Count > 0)
            {
                query = query.Where(p => p.Tags != null && p.Tags.Any(t => productFilterDto.Tags.Contains(t)));
            }

            // Lọc theo khoảng giá
            if (productFilterDto.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= productFilterDto.MinPrice.Value);
            }
            if (productFilterDto.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= productFilterDto.MaxPrice.Value);
            }

            // Lọc theo số lượng tồn kho
            if (productFilterDto.MinStockQuantity.HasValue)
            {
                query = query.Where(p => p.StockQuantity >= productFilterDto.MinStockQuantity.Value);
            }

            if (productFilterDto.MaxStockQuantity.HasValue)
            {
                query = query.Where(p => p.StockQuantity <= productFilterDto.MaxStockQuantity.Value);
            }

            // Phân trang
            query = query.Skip((pageIndex - 1) * limit).Take(limit);

            // Thực thi truy vấn và chuyển đổi sang DTO
            IEnumerable<Products> products = await query.ToListAsync();
            IEnumerable<ProductDto> productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);

            // Lấy tổng số sản phẩm để tính toán phân trang
            long totalCount = await _unitOfWork.GetCollection<Products>()
                .CountDocumentsAsync(p => (string.IsNullOrEmpty(productFilterDto.SearchTerm) || p.Name.Contains(productFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase) || p.Description.Contains(productFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase)) &&
                                          (!productFilterDto.CategoryIds.Any() || (p.CategoryIds != null && p.CategoryIds.Any(c => productFilterDto.CategoryIds.Contains(c)))) &&
                                          (string.IsNullOrEmpty(productFilterDto.Brand) || p.Brand.Contains(productFilterDto.Brand, StringComparison.CurrentCultureIgnoreCase)) &&
                                          (!productFilterDto.Tags.Any() || (p.Tags != null && p.Tags.Any(t => productFilterDto.Tags.Contains(t)))) &&
                                          (!productFilterDto.MinPrice.HasValue || p.Price >= productFilterDto.MinPrice.Value) &&
                                          (!productFilterDto.MaxPrice.HasValue || p.Price <= productFilterDto.MaxPrice.Value) &&
                                          (!productFilterDto.MinStockQuantity.HasValue || p.StockQuantity >= productFilterDto.MinStockQuantity.Value) &&
                                          (!productFilterDto.MaxStockQuantity.HasValue || p.StockQuantity <= productFilterDto.MaxStockQuantity.Value));

            return new PaginatedResult<ProductDto>
            {
                Items = productsDto,
                TotalCount = totalCount,
            };
        }

        public async Task<ProductDto> GetProductByIdAsync(string id)
        {
            Products products = await _unitOfWork.GetCollection<Products>().Find(p => p.Id == id).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Product not found");

            ProductDto productDto = _mapper.Map<ProductDto>(products);

            return productDto;
        }

        public async Task CreateProductAsync(CreateProductDto productDto)
        {
            // Kiểm tra xem sản phẩm đã tồn tại chưa
            bool productExists = await _unitOfWork.GetCollection<Products>().Find(p => p.Name == productDto.Name).AnyAsync();
            if (productExists)
            {
                throw new BadRequestCustomException("Product already exists");
            }

            // Upload ảnh lên Cloudinary
            List<string> imageUrls = [];
            if (productDto.ImageProduct is not null && productDto.ImageProduct.Count > 0)
            {
                foreach (IFormFile image in productDto.ImageProduct)
                {
                    ImageUploadResult result = _cloudinaryService.UploadImage(image, ImageTag.Product);
                    imageUrls.Add(result.SecureUrl.AbsoluteUri);
                }
            }

            Products product = new()
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                CategoryIds = productDto.CategoryIds,
                Brand = productDto.Brand,
                Tags = productDto.Tags,
                StockQuantity = productDto.StockQuantity,
                ImageUrls = imageUrls,
                NutritionFact = productDto.NutritionFact,
                CreatedAt = TimeControl.GetUtcPlus7Time(),
                UpdatedAt = null
            };

            await _unitOfWork.GetCollection<Products>().InsertOneAsync(product);
        }

        public async Task<List<Products>> GetProductsByPriceRangeAsync(double minPrice, double maxPrice)
        {
            return await _unitOfWork.GetCollection<Products>()
                .Find(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToListAsync();
        }


        public async Task UpdateProductAsync(string id, UpdateProductDto updateProductDto)
        {
            Products product = await _unitOfWork.GetCollection<Products>().Find(p => p.Id == id).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Product not found");

            UpdateDefinitionBuilder<Products> updateBuilder = Builders<Products>.Update;

            List<UpdateDefinition<Products>> updates =
            [
                updateBuilder.Set(p => p.Name, updateProductDto.Name),
                updateBuilder.Set(p => p.Description, updateProductDto.Description),
                updateBuilder.Set(p => p.Price, updateProductDto.Price),
                updateBuilder.Set(p => p.CategoryIds, updateProductDto.CategoryIds),
                updateBuilder.Set(p => p.Brand, updateProductDto.Brand),
                updateBuilder.Set(p => p.Tags, updateProductDto.Tags),
                updateBuilder.Set(p => p.StockQuantity, updateProductDto.StockQuantity),
                updateBuilder.Set(p => p.NutritionFact, updateProductDto.NutritionFact),
                updateBuilder.Set(p => p.UpdatedAt, TimeControl.GetUtcPlus7Time())
            ];

            // Fix for CS1061: 'UpdateProductDto' does not contain a definition for 'Image'.
            // The error occurs because the `UpdateProductDto` class does not have a property named `Image`.
            // Based on the context, it seems the intended property is `ImageProduct`.
            // Update the code to use `ImageProduct` instead of `Image`.
            if (updateProductDto.ImageProduct is not null && updateProductDto.ImageProduct.Count > 0)
            {
                // Upload images to Cloudinary
                foreach (IFormFile image in updateProductDto.ImageProduct)
                {
                    ImageUploadResult result = _cloudinaryService.UploadImage(image, ImageTag.Product);

                    // Update the first image URL in the product's ImageUrls list
                    updates.Add(updateBuilder.Set(p => p.ImageUrls[0], result.SecureUrl.AbsoluteUri));
                }
            }

            UpdateDefinition<Products> updateDefinition = updateBuilder.Combine(updates);

            await _unitOfWork.GetCollection<Products>().FindOneAndUpdateAsync(product => product.Id == id, updateDefinition);
        }

        public async Task DeleteProductAsync(string id)
        {
            await _unitOfWork.GetCollection<Products>().DeleteOneAsync(p => p.Id == id);
        }
    }
}
