using CloudinaryDotNet.Actions;
using HealthyNutritionApp.Application.Dto;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Models;
using HealthyNutritionApp.Domain.Enums;
using HealthyNutritionApp.Domain.Utils;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace HealthyNutritionApp.Infrastructure.Implements.Account
{
    public class AccountService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ICloudinaryService cloudinaryService) : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ICloudinaryService _cloudinaryService = cloudinaryService;

        #region GetUserProfile
        public async Task<UserProfileDto> GetUserProfileAsync()
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Your session is limit, you must login again to edit profile!");
            }

            // Lấy thông tin người dùng từ DB
            Users user = await _unitOfWork.GetCollection<Users>().Find(user => user.Id == userId).FirstOrDefaultAsync() ?? throw new Exception("User not found");

            // Chuyển đổi sang DTO
            UserProfileDto userProfileDto = new()
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Image = user.Image,
            };

            return userProfileDto;
        }
        #endregion

        #region EditProfile
        public async Task EditProfileAsync(EditProfileDto editProfileDto)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Your session is limit, you must login again to edit profile!");
            }

            Users user = await _unitOfWork.GetCollection<Users>().Find(user => user.Id == editProfileDto.UserId).FirstOrDefaultAsync() ?? throw new Exception("User not found");

            // Validate input parameters
            ValidateInput(editProfileDto.UserId, editProfileDto.FullName, editProfileDto.PhoneNumber);

            // Build update definition
            UpdateDefinitionBuilder<Users> updateBuilder = Builders<Users>.Update;

            // Cập nhật field sẵn có
            List<UpdateDefinition<Users>> updates =
            [
                updateBuilder.Set(user => user.FullName, editProfileDto.FullName),
                updateBuilder.Set(user => user.PhoneNumber, editProfileDto.PhoneNumber),
                updateBuilder.Set(user => user.Address, editProfileDto.Address),
                updateBuilder.Set(user => user.UpdatedAt, TimeControl.GetUtcPlus7Time())
            ];

            // Cập nhật Image Field nếu có
            if (editProfileDto.Image is not null)
            {
                // Upload ảnh lên Cloudinary
                ImageUploadResult result = _cloudinaryService.UploadImage(editProfileDto.Image, ImageTag.Users_Profile);

                // Cập nhật URL cho ảnh
                updates.Add(updateBuilder.Set(user => user.Image, result.SecureUrl.AbsoluteUri));
            }

            UpdateDefinition<Users> updateDefinition = updateBuilder.Combine(updates);

            // Cập nhật thông tin người dùng
            await _unitOfWork.GetCollection<Users>()
                .FindOneAndUpdateAsync(user => user.Id == editProfileDto.UserId, updateDefinition);
        }
        #endregion

        private void ValidateInput(string userId, string fullName, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Invalid UserId parameters");
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Invalid FullName parameters");
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Invalid PhoneNumber parameters");
            }
        }
    }
}
