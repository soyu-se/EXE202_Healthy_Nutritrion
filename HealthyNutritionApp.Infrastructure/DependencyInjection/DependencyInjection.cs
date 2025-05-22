using HealthyNutritionApp.Application.DatabaseContext;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Mapper;
using HealthyNutritionApp.Infrastructure.Implements;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace HealthyNutritionApp.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddDependencyInjection(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            services.ConfigRoute();
            //services.AddSwaggerGen();
        }

        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            services.Configure<DataProtectionTokenProviderOptions>(otp =>
            {
                otp.TokenLifespan = TimeSpan.FromMinutes(3);
            });
        }

        public static void AddDatabase(this IServiceCollection services)
        {
            // Load MongoDB settings from environment variables
            string connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");

            // Register the MongoDB settings as a singleton
            var mongoDbSettings = new MongoDbSetting
            {
                ConnectionString = connectionString,
                DatabaseName = databaseName
            };

            // Register the MongoDBSetting with DI
            services.AddSingleton(mongoDbSettings);

            // Register MongoClient as singleton, sharing the connection across all usages
            services.AddSingleton<IMongoClient>(sp =>
            {
                return new MongoClient(mongoDbSettings.ConnectionString);
            });
            //services.AddSingleton<IMongoClient>(_lazyClient.Value);

            // Register IMongoDatabase as a scoped service
            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDbSettings.DatabaseName);
            });

            // Register the MongoDB context (or client)
            services.AddSingleton<HealthyNutritionDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
