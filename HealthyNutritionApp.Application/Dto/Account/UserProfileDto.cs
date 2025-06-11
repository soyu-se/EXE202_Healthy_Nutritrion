using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;
using System.Text.Json.Serialization;

namespace HealthyNutritionApp.Application.Dto.Account
{
    public class UserProfileDto : IMapFrom<Users>
    {
        public string? Id { get; set; } // Unique identifier for the user
        public string? FullName { get; set; }
        public string? Email { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PhoneNumber { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Address { get; set; }
        public string? Image { get; set; } // URL of the image
        public string? Role { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? CreatedAt { get; set; }
    }
}
