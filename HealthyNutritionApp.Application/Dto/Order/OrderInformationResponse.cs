using AutoMapper;
using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Order
{
    public class OrderInformationResponse : IMapFrom<Orders>
    {
        public int OrderCode { get; set; }
        public List<OrderItems> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OrderInformationResponse, Orders>()
                   .ForMember(dest => dest.PayOSOrderCode, opt => opt.MapFrom(oir => oir.OrderCode))
                   .ForMember(dest => dest.Items, opt => opt.MapFrom(oir => oir.Items))
                   .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(oir => oir.TotalAmount))
                   .ForMember(dest => dest.Status, opt => opt.MapFrom(oir => oir.Status))
                   .ReverseMap();
        }
    }
}
