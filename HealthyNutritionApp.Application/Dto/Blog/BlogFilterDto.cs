namespace HealthyNutritionApp.Application.Dto.Blog
{
    public class BlogFilterDto
    {
        public string? SearchTerm { get; set; } // Từ khóa tìm kiếm bài viết
        public List<string>? Tags { get; set; } // Danh sách thẻ bài viết
        public DateTime? StartDate { get; set; } // Ngày bắt đầu lọc
        public DateTime? EndDate { get; set; } // Ngày kết thúc lọc
    }
}
