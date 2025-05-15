using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HealthyNutritionApp.Application.Mapper
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType()).ReverseMap();
    }
}
