using AutoMapper;
using HealthyNutritionApp.Application.Dto.Feedback;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Feedback;
using HealthyNutritionApp.Domain.Utils;
using MongoDB.Driver;

namespace HealthyNutritionApp.Infrastructure.Implements.Feedback
{
    public class FeedbackService(IUnitOfWork unitOfWork, IMapper mapper) : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<PaginatedResult<CreateFeedbackDto>> GetFeedbacksAsync(int offset, int limit)
        {
            // Here you would typically retrieve feedback from a database or another storage system.
            // For this example, we will just return an empty list.
            List<Domain.Entities.Feedback> feedbacks = await _unitOfWork.GetCollection<Domain.Entities.Feedback>()
                .Find(_ => true)
                .Skip((offset - 1) * limit)
                .Limit(limit)
                .ToListAsync();

            long totalCount = await _unitOfWork.GetCollection<Domain.Entities.Feedback>().CountDocumentsAsync(_ => true);

            IEnumerable<CreateFeedbackDto> feedbacksDto = _mapper.Map<IEnumerable<CreateFeedbackDto>>(feedbacks);

            return new PaginatedResult<CreateFeedbackDto>
            {
                Items = feedbacksDto,
                TotalCount = totalCount,
            };
        }

        public async Task<long> GetTotalCountFeedbackAsync()
        {
            long totalCount = await _unitOfWork.GetCollection<Domain.Entities.Feedback>().CountDocumentsAsync(_ => true);

            return totalCount;
        }

        public async Task<CreateFeedbackDto> GetFeedbackByIdAsync(string feedbackId)
        {
            if (string.IsNullOrWhiteSpace(feedbackId))
            {
                throw new BadRequestCustomException("Feedback ID cannot be null or empty.");
            }

            // Here you would typically retrieve feedback from a database or another storage system.
            // For this example, we will just simulate the operation with a completed task.
            FilterDefinition<Domain.Entities.Feedback> filter = Builders<Domain.Entities.Feedback>.Filter.Eq(f => f.Id, feedbackId);

            Domain.Entities.Feedback feedback = await _unitOfWork.GetCollection<Domain.Entities.Feedback>().Find(filter).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Feedback not found.");

            return _mapper.Map<CreateFeedbackDto>(feedback);
        }

        public async Task SubmitFeedbackAsync(CreateFeedbackDto createFeedbackDto)
        {
            if (string.IsNullOrWhiteSpace(createFeedbackDto.Content))
            {
                throw new ArgumentException("Feedback text cannot be null or empty.");
            }

            // Here you would typically save the feedback to a database or another storage system.
            // For this example, we will just simulate the operation with a completed task.
            Domain.Entities.Feedback feedback = new()
            {
                Content = createFeedbackDto.Content,
                CreatedAt = TimeControl.GetUtcPlus7Time(),
                UpdatedAt = null
            };

            await _unitOfWork.GetCollection<Domain.Entities.Feedback>().InsertOneAsync(feedback);

            return;
        }

        public async Task DeleteFeedbackAsync(string feedbackId)
        {
            if (string.IsNullOrWhiteSpace(feedbackId))
            {
                throw new BadRequestCustomException("Feedback ID cannot be null or empty.");
            }

            // Here you would typically delete the feedback from a database or another storage system.
            // For this example, we will just simulate the operation with a completed task.
            var filter = Builders<Domain.Entities.Feedback>.Filter.Eq(f => f.Id, feedbackId);
            var result = await _unitOfWork.GetCollection<Domain.Entities.Feedback>().DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                throw new NotFoundCustomException("Feedback not found.");
            }
        }
    }
}
