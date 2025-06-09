using Microsoft.AspNetCore.Http;

namespace HealthyNutritionApp.Application.Dto.Blog
{
    public class UpdateBlogDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Excerpt { get; set; }
        public List<string> Tags { get; set; }
        public IFormFileCollection? ImageBlog { get; set; }
    }
}
