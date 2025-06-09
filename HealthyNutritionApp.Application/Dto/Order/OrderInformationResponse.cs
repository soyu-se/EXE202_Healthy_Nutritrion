using AutoMapper;
using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;

namespace HealthyNutritionApp.Application.Dto.Order
{
    public class OrderInformationResponse : IMapFrom<Orders>
    {
        public int OrderCode { get; set; }
        public List<OrderItemsDto> Items { get; set; }
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

    public class  OrderItemsDto : IMapFrom<OrderItems>
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal PricePerUnit { get; set; }

        public string ProductImageUrl { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OrderItemsDto, OrderItems>()
                   .ForMember(dest => dest.ProductId, opt => opt.MapFrom(oir => oir.ProductId))
                   .ForMember(dest => dest.ProductName, opt => opt.MapFrom(oir => oir.ProductName))
                   .ForMember(dest => dest.Quantity, opt => opt.MapFrom(oir => oir.Quantity))
                   .ForMember(dest => dest.PricePerUnit, opt => opt.MapFrom(oir => oir.PricePerUnit))
                   .ReverseMap();
        }
    }
}
