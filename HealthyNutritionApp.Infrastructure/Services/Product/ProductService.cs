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
                query = query.Where(p => p.Name.Contains(productFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase));
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

            // Nếu danh sách sản phẩm rỗng, trả về kết quả rỗng
            if (!productsDto.Any())
            {
                return new PaginatedResult<ProductDto>
                {
                    Items = productsDto.ToList(),
                    TotalCount = 0,
                };
            }

            // Lấy tổng số sản phẩm để tính toán phân trang
            long totalCount = await _unitOfWork.GetCollection<Products>()
                .CountDocumentsAsync(p => (string.IsNullOrEmpty(productFilterDto.SearchTerm) ||
                                           p.Name.Contains(productFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                                           p.Description.Contains(productFilterDto.SearchTerm, StringComparison.CurrentCultureIgnoreCase)) &&
                                          (productFilterDto.CategoryIds == null || !productFilterDto.CategoryIds.Any() ||
                                           (p.CategoryIds != null && p.CategoryIds.Any(c => productFilterDto.CategoryIds.Contains(c)))) &&
                                          (string.IsNullOrEmpty(productFilterDto.Brand) ||
                                           p.Brand.Contains(productFilterDto.Brand, StringComparison.CurrentCultureIgnoreCase)) &&
                                          (productFilterDto.Tags == null || !productFilterDto.Tags.Any() ||
                                           (p.Tags != null && p.Tags.Any(t => productFilterDto.Tags.Contains(t)))) &&
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
                Rating = 0,
                ReviewCount = 0,
                CreatedAt = TimeControl.GetUtcPlus7Time(),
                UpdatedAt = null
            };

            await _unitOfWork.GetCollection<Products>().InsertOneAsync(product);
        }

        public async Task UpdateProductAsync(string id, UpdateProductDto updateProductDto)
        {
            Products product = await _unitOfWork.GetCollection<Products>().Find(p => p.Id == id).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Product not found");

            UpdateDefinitionBuilder<Products> updateBuilder = Builders<Products>.Update;
            List<UpdateDefinition<Products>> updates = [];

            // Only update fields if they are provided (non-null)
            if (updateProductDto.Name != null)
            {
                updates.Add(updateBuilder.Set(p => p.Name, updateProductDto.Name));
            }
            
            if (updateProductDto.Description != null)
            {
                updates.Add(updateBuilder.Set(p => p.Description, updateProductDto.Description));
            }
            
            // For numeric types like double and int, we need to check if the DTO was actually provided with values
            // As these cannot be null. For Price and StockQuantity, we'll assume non-zero/non-negative values are intentional updates.
            if (updateProductDto.Price > 0)
            {
                updates.Add(updateBuilder.Set(p => p.Price, updateProductDto.Price));
            }
            
            if (updateProductDto.StockQuantity >= 0) // Allow setting stock to 0
            {
                updates.Add(updateBuilder.Set(p => p.StockQuantity, updateProductDto.StockQuantity));
            }
            
            if (updateProductDto.CategoryIds != null)
            {
                updates.Add(updateBuilder.Set(p => p.CategoryIds, updateProductDto.CategoryIds));
            }
            
            if (updateProductDto.Brand != null)
            {
                updates.Add(updateBuilder.Set(p => p.Brand, updateProductDto.Brand));
            }
            
            if (updateProductDto.Tags != null)
            {
                updates.Add(updateBuilder.Set(p => p.Tags, updateProductDto.Tags));
            }
            
            if (updateProductDto.NutritionFact != null)
            {
                updates.Add(updateBuilder.Set(p => p.NutritionFact, updateProductDto.NutritionFact));
            }
            
            // Always update the timestamp when making changes
            updates.Add(updateBuilder.Set(p => p.UpdatedAt, TimeControl.GetUtcPlus7Time()));

            // Handle image uploads if provided
            if (updateProductDto.ImageProduct != null && updateProductDto.ImageProduct.Count > 0)
            {
                List<string> imageUrls = [];
                
                // Upload each image to Cloudinary
                foreach (IFormFile image in updateProductDto.ImageProduct)
                {
                    ImageUploadResult result = _cloudinaryService.UploadImage(image, ImageTag.Product);
                    imageUrls.Add(result.SecureUrl.AbsoluteUri);
                }
                
                // Update the entire ImageUrls collection if we have new images
                if (imageUrls.Count > 0)
                {
                    updates.Add(updateBuilder.Set(p => p.ImageUrls, imageUrls));
                }
            }

            if (updates.Count > 0)
            {
                UpdateDefinition<Products> updateDefinition = updateBuilder.Combine(updates);
                await _unitOfWork.GetCollection<Products>().FindOneAndUpdateAsync(product => product.Id == id, updateDefinition);
            }
        }

        public async Task DeleteProductAsync(string id)
        {
            await _unitOfWork.GetCollection<Products>().DeleteOneAsync(p => p.Id == id);
        }
    }
}
