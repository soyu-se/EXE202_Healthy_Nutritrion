using CloudinaryDotNet.Actions;
using HealthyNutritionApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HealthyNutritionApp.Application.ThirdPartyService.Cloudinary
{
    public interface ICloudinaryService
    {
        public ImageUploadResult UploadImage(IFormFile imageFile, ImageTag imageTag, string rootFolder = "Image");
    }
}
