using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthyNutritionApp.Application.Projections.Lookup.Product
{
    [BsonIgnoreExtraElements]
    public class ProductProjection : IMapFrom<Products>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;

        public IEnumerable<CategoryProjection> Categories { get; set; } = [];
    }

    [BsonIgnoreExtraElements]
    public class CategoryProjection
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}
