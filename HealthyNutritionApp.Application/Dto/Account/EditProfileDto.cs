using Microsoft.AspNetCore.Http;

namespace HealthyNutritionApp.Application.Dto.Account
{
    public class EditProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public IFormFile? Image { get; set; } // URL of the image
    }
}
