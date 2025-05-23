using System.Runtime.Serialization;

namespace HealthyNutritionApp.Domain.Enums
{
    public enum ImageTag
    {
        [EnumMember(Value = "Users_Profile")]
        Users_Profile,

        [EnumMember(Value = "Product")]
        Product
    }
}
