using MongoDB.Driver;

namespace HealthyNutritionApp.Application.DatabaseContext
{
    public class HealthyNutritionDbContext(MongoDbSetting mongoDBSettings, IMongoClient mongoClient)
    {

        private readonly IMongoDatabase _database = mongoClient.GetDatabase(mongoDBSettings.DatabaseName);

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }
    }

}
