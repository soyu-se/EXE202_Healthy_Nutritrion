using AutoMapper;

namespace HealthyNutritionApp.Application.Mapper
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType()).ReverseMap();
    }
}
