using HealthyNutritionApp.Infrastructure.DependencyInjection;
using Serilog;
namespace HealthyNutritionApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load environment variables from .env file
            EnvironmentVariableLoader.LoadEnvironmentVariable();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDependencyInjection();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .WithOrigins("http://localhost:5173")
                        .WithOrigins(Environment.GetEnvironmentVariable("HEALTHY_NUTRITION_CLIENT_URL") ?? throw new Exception("ClientUrl connect fail")) //throw new InvalidDataCustomException("SPOTIFYPOOL_CLIENT_URL is not set");  ////SERI LOG
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration); // đọc từ appsettings.json
            });

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.File(@"F:\Logs\AEM\log.txt"))
                .CreateLogger();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    // Đặt tiêu đề
                    options.DocumentTitle = "HealthyNutrition API";

                    //    // Đường dẫn đến file JSON của Swagger
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthyNutrition API V1");

                    //    // Inject JavaScript để chuyển đổi theme
                    options.InjectJavascript("/theme-switcher.js");
                });
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCors("AllowSpecificOrigin");

            app.MapControllers();

            app.Run();
        }
    }
}
