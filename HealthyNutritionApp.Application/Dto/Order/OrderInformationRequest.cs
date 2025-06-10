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

        // kilogram sản phẩm
        public double Weight { get; set; }
        public int Quantity { get; set; }
        public int PricePerKilogram { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OrderItems, CartItems>()
                   .ForMember(dest => dest.ProductId, opt => opt.MapFrom(o => o.ProductId))
                   .ForMember(dest => dest.ProductName, opt => opt.MapFrom(o => o.ProductName))
                   .ForMember(dest => dest.Quantity, opt => opt.MapFrom(o => o.Quantity))
                   .ForMember(dest => dest.Weight, opt => opt.MapFrom(o => o.Weight))
                   .ForMember(dest => dest.PricePerKilogram, opt => opt.MapFrom(o => o.PricePerKilogram))
                   .ReverseMap();
        }
    }
}
