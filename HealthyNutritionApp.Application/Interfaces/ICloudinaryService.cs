using CloudinaryDotNet.Actions;
using HealthyNutritionApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HealthyNutritionApp.Application.Interfaces
{
    public interface ICloudinaryService
    {
        public ImageUploadResult UploadImage(IFormFile imageFile, ImageTag imageTag, string rootFolder = "Image");
    }
}
