using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Review
{
    public class ReviewWithUserDto
    {
        public UserInfo User { get; set; }
        
        public class UserInfo
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Image { get; set; }
            public string Email { get; set; }
        }
    }
}