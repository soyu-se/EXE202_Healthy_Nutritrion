using HealthyNutritionApp.Application.Dto.Order;
using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Transaction
{
    public class TransactionRevenueByDayResponse : IMapFrom<Transactions>
    {
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }

        public void Mapping(AutoMapper.Profile profile)
        {
            profile.CreateMap<Transactions, TransactionRevenueByDayResponse>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.CreatedAt)))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.OrderAmount))
                .ReverseMap();
        }
    }
}
