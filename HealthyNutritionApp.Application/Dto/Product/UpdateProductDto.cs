using HealthyNutritionApp.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace HealthyNutritionApp.Application.Dto.Product
{
    public class UpdateProductDto
    {
        public string Name { get; set; } // Tên sản phẩm
        public string Description { get; set; } // Mô tả sản phẩm
        public string Brand { get; set; } // Thương hiệu sản phẩm
        public double Price { get; set; } // Giá sản phẩm
        public int StockQuantity { get; set; } // Số lượng tồn kho
        public List<string> CategoryIds { get; set; } // ID danh mục sản phẩm
        public List<string> Tags { get; set; } = []; // Danh sách thẻ sản phẩm
        public NutritionFact NutritionFact { get; set; } // Thông tin dinh dưỡng sản phẩm
        public IFormFileCollection? ImageProduct { get; set; } // Hình ảnh sản phẩm
    }
}
