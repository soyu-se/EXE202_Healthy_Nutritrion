using HealthyNutritionApp.Application.Mapper;

namespace HealthyNutritionApp.Application.Dto.Feedback
{
    public class CreateFeedbackDto : IMapFrom<Domain.Entities.Feedback>
    {
        public string Content { get; set; }
    }
}
