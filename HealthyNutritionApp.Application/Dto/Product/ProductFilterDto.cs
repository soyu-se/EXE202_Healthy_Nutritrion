namespace HealthyNutritionApp.Application.Dto.Product
{
    public class ProductFilterDto
    {
        public string? SearchTerm { get; set; } // Từ khóa tìm kiếm sản phẩm
        public List<string>? CategoryIds { get; set; } // Danh sách ID danh mục sản phẩm
        public string? Brand { get; set; } // Thương hiệu sản phẩm
        public List<string>? Tags { get; set; } // Danh sách thẻ sản phẩm
        public double? MinPrice { get; set; } // Giá tối thiểu
        public double? MaxPrice { get; set; } // Giá tối đa
        public int? MinStockQuantity { get; set; } // Số lượng tồn kho tối thiểu
        public int? MaxStockQuantity { get; set; } // Số lượng tồn kho tối đa
        //public int Offset { get; set; } = 1; // Vị trí bắt đầu phân trang
        //public int Limit { get; set; } = 10; // Số lượng sản phẩm mỗi trang
    }
}
