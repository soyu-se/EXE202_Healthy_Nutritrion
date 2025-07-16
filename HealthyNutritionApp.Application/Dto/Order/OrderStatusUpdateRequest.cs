using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Order
{
    public class OrderStatusUpdateRequest : IMapFrom<Orders>
    {
        public int OrderCode { get; set; }
        public string Status { get; set; }

        public void Mapping(AutoMapper.Profile profile)
        {
            profile.CreateMap<OrderStatusUpdateRequest, Orders>()
                .ForMember(dest => dest.PayOSOrderCode, opt => opt.MapFrom(osur => osur.OrderCode))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(osur => osur.Status))
                .ReverseMap();
        }
    }
}
