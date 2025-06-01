using AutoMapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Order
{
    public class OrderListResponse
    {
        public int OrderCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OrderInformationResponse, Orders>()
                   .ForMember(dest => dest.PayOSOrderCode, opt => opt.MapFrom(oir => oir.OrderCode))
                   .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(oir => oir.TotalAmount))
                   .ForMember(dest => dest.Status, opt => opt.MapFrom(oir => oir.Status))
                   .ReverseMap();
        }
    }
}
