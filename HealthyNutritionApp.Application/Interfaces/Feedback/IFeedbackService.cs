using HealthyNutritionApp.Application.Dto.Feedback;
using HealthyNutritionApp.Application.Dto.PaginatedResult;

namespace HealthyNutritionApp.Application.Interfaces.Feedback
{
    public interface IFeedbackService
    {
        // Define methods for feedback service here
        Task<PaginatedResult<CreateFeedbackDto>> GetFeedbacksAsync(int offset = 1, int limit = 10);
        Task SubmitFeedbackAsync(CreateFeedbackDto createFeedbackDto);
        Task DeleteFeedbackAsync(string feedbackId);
        Task<CreateFeedbackDto> GetFeedbackByIdAsync(string feedbackId);
        Task<long> GetTotalCountFeedbackAsync();
    }
}
