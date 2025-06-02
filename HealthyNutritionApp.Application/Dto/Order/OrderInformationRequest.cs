
using AutoMapper;
using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Order
{
    public class OrderInformationRequest : IMapFrom<Orders>
    {
        public List<CartItems> Items { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OrderInformationRequest, Orders>()
                    .ForMember(dest => dest.Items, opt => opt.MapFrom(o => o.Items))
                    .ReverseMap();
        }
    }

    public class CartItems : IMapFrom<OrderItems>
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int PricePerUnit { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CartItems, OrderItems>().ReverseMap();
        }
    }
}
