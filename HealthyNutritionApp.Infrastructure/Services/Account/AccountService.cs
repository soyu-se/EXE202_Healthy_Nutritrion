using AutoMapper;
using CloudinaryDotNet.Actions;
using HealthyNutritionApp.Application.Dto.Account;
using HealthyNutritionApp.Application.Dto.PaginatedResult;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Account;
using HealthyNutritionApp.Application.ThirdPartyServices.Cloudinary;
using HealthyNutritionApp.Domain.Entities;
using HealthyNutritionApp.Domain.Enums;
using HealthyNutritionApp.Domain.Utils;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace HealthyNutritionApp.Infrastructure.Services.Account
{
    public class AccountService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ICloudinaryService cloudinaryService, IMapper mapper) : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
        private readonly IMapper _mapper = mapper;

        #region Get All Users
        public async Task<long> GetTotalCountUsersAsync()
        {
            // Lấy tổng số lượng người dùng từ DB
            long totalCount = await _unitOfWork.GetCollection<Users>().CountDocumentsAsync(_ => true);

            // Trả về tổng số lượng người dùng
            return totalCount;
        }
        #endregion

        #region GetAllUsers
        public async Task<PaginatedResult<UserAccountDto>> GetUsersAsync(int pageIndex, int limit)
        {
            // Lấy danh sách người dùng từ DB với phân trang
            IEnumerable<Users> users = await _unitOfWork.GetCollection<Users>()
                .Find(_ => true)
                .Skip((pageIndex - 1) * limit)
                .Limit(limit)
                .ToListAsync();

            long totalCount = await _unitOfWork.GetCollection<Users>().CountDocumentsAsync(_ => true);

            // Chuyển đổi sang danh sách DTO
            IEnumerable<UserAccountDto> userAccountsDto = _mapper.Map<IEnumerable<UserAccountDto>>(users);

            return new PaginatedResult<UserAccountDto>
            {
                Items = userAccountsDto,
                TotalCount = totalCount
            };
        }
        #endregion

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
            UserProfileDto userProfileDto = _mapper.Map<UserProfileDto>(user);

            return userProfileDto;
        }
        #endregion

        #region CreateUserAsync
        public async Task CreateUserAsync(CreateUserDto createUserDto)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(createUserDto.FullName))
            {
                throw new BadRequestCustomException("Full name is required");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.PhoneNumber))
            {
                throw new BadRequestCustomException("Phone number is required");
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Password))
            {
                throw new BadRequestCustomException("Password is required");
            }

            // Kiểm tra xem người dùng đã tồn tại chưa
            Users existingUser = await _unitOfWork.GetCollection<Users>().Find(user => user.PhoneNumber == createUserDto.PhoneNumber).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new Exception("User already exists with this phone number");
            }

            // Tạo người dùng mới
            Users newUser = new()
            {
                FullName = createUserDto.FullName,
                Email = createUserDto.Email,
                PhoneNumber = createUserDto.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password), // Mã hóa mật khẩu nếu cần
                Role = "User",
                Image = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730097883/60d5dc467b950c5ccc8ced95_spotify-for-artists_on4me9.jpg", // Có thể là null nếu không có ảnh
                CreatedAt = TimeControl.GetUtcPlus7Time(),
                UpdatedAt = null
            };

            // Lưu người dùng vào DB
            await _unitOfWork.GetCollection<Users>().InsertOneAsync(newUser);
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

            Users user = await _unitOfWork.GetCollection<Users>().Find(user => user.Id == userId).FirstOrDefaultAsync() ?? throw new Exception("User not found");

            // Build update definition
            UpdateDefinitionBuilder<Users> updateBuilder = Builders<Users>.Update;
            List<UpdateDefinition<Users>> updates = [];

            // Only update fields that are not null
            if (editProfileDto.FullName is not null)
            {
                updates.Add(updateBuilder.Set(u => u.FullName, editProfileDto.FullName));
            }
            
            if (editProfileDto.PhoneNumber is not null)
            {
                updates.Add(updateBuilder.Set(u => u.PhoneNumber, editProfileDto.PhoneNumber));
            }
            
            if (editProfileDto.Address is not null)
            {
                updates.Add(updateBuilder.Set(u => u.Address, editProfileDto.Address));
            }

            // Always update the UpdatedAt timestamp if any changes are being made
            updates.Add(updateBuilder.Set(u => u.UpdatedAt, TimeControl.GetUtcPlus7Time()));

            // Cập nhật Image Field nếu có
            if (editProfileDto.Image is not null)
            {
                // Upload ảnh lên Cloudinary
                ImageUploadResult result = _cloudinaryService.UploadImage(editProfileDto.Image, ImageTag.Users_Profile);

                // Cập nhật URL cho ảnh
                updates.Add(updateBuilder.Set(u => u.Image, result.SecureUrl.AbsoluteUri));
            }

            if (updates.Count > 1) // > 1 because we always add UpdatedAt
            {
                UpdateDefinition<Users> updateDefinition = updateBuilder.Combine(updates);

                // Cập nhật thông tin người dùng
                await _unitOfWork.GetCollection<Users>()
                    .FindOneAndUpdateAsync(user => user.Id == userId, updateDefinition);
            }
        }
        #endregion

        #region DeleteUserAsync
        public async Task DeleteUserAsync(string userId)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Invalid UserId parameters");
            }
            // Xóa người dùng khỏi DB
            DeleteResult result = await _unitOfWork.GetCollection<Users>().DeleteOneAsync(user => user.Id == userId);
            if (result.DeletedCount == 0)
            {
                throw new NotFoundCustomException("User not found");
            }
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
