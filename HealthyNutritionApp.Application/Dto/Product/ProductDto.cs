using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Product
{
    public class ProductDto : IMapFrom<Products>
    {
        public string Id { get; set; } = string.Empty; // Mã sản phẩm
        public string Name { get; set; } = string.Empty; // Tên sản phẩm
        public string Description { get; set; } = string.Empty; // Mô tả sản phẩm
        public double Price { get; set; } // Giá sản phẩm
        public int StockQuantity { get; set; } // Số lượng tồn kho
        public List<string> ImageUrls { get; set; } = []; // Danh sách URL hình ảnh sản phẩm
        public List<string> CategoryIds { get; set; } // ID danh mục sản phẩm
        public List<string> Tags { get; set; } = []; // Danh sách thẻ sản phẩm
        public string Brand { get; set; } // Thương hiệu sản phẩm
        public NutritionFact NutritionFact { get; set; } // Thông tin dinh dưỡng của sản phẩm
        public double Rating { get; set; } // Điểm đánh giá trung bình của sản phẩm
        public long ReviewCount { get; set; } // Tổng số đánh giá của sản phẩm
    }
}
