using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Account
{
    public class AccountRegisterCountingResponse : IMapFrom<Users>
    {
        public DateOnly Date { get; set; }
        public long SignUpNumber { get; set; }

        public void Mapping(AutoMapper.Profile profile)
        {
            profile.CreateMap<Users, AccountRegisterCountingResponse>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.CreatedAt)))
                .ReverseMap();
        }
    }
}
