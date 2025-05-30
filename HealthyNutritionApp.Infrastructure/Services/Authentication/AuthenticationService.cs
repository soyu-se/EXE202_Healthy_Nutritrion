using HealthyNutritionApp.Application.Dto.Authentication;
using HealthyNutritionApp.Application.Exceptions;
using HealthyNutritionApp.Application.Interfaces;
using HealthyNutritionApp.Application.Interfaces.Authentication;
using HealthyNutritionApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Security.Claims;

namespace HealthyNutritionApp.Infrastructure.Services.Authentication
{
    public class AuthenticationService(IUnitOfWork unitOfWork, IJsonWebToken jsonWebToken, IHttpContextAccessor httpContextAccessor) : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IJsonWebToken _jsonWebToken = jsonWebToken;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        #region Register
        public async Task CreateAccountAsync(RegisterAccountDto registerUser)
        {
            string email = registerUser.Email;
            string password = registerUser.Password;
            string confirmPassword = registerUser.ConfirmPassword;
            string hashedPassword = HashPassword(registerUser.Password);
            string fullName = registerUser.FullName;
            string phoneNumber = registerUser.PhoneNumber;
            string address = registerUser.Address;

            // Kiểm tra mật khẩu xác nhận
            bool isConfirmedPassword = password == confirmPassword;
            if (!isConfirmedPassword)
            {
                throw new BadRequestCustomException("Password and Confirm Password do not match");
            }

            // Kiểm tra account đã tồn tại hay chưa
            if (await IsEmailExisted(email))
            {
                throw new ConflictCustomException("Account already exists");
            }

            if (await IsPhoneNumberExisted(phoneNumber))
            {
                throw new ConflictCustomException("Phone number already exists");
            }

            // Tạo tài khoản mới
            Users newUser = new()
            {
                FullName = fullName,
                Image = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730097883/60d5dc467b950c5ccc8ced95_spotify-for-artists_on4me9.jpg", // Default image URL
                Email = email,
                Password = hashedPassword,
                Address = address,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdatedAt = DateTime.UtcNow.AddHours(7),
                Role = "User" // Default role
            };

            await _unitOfWork.GetCollection<Users>().InsertOneAsync(newUser);
        }

        private async Task<bool> IsEmailExisted(string email)
        {
            return await _unitOfWork.GetCollection<Users>()
                .Find(user => user.Email == email)
                .Project(user => user.Email)
                .AnyAsync();
        }

        private async Task<bool> IsPhoneNumberExisted(string phoneNumber)
        {
            return await _unitOfWork.GetCollection<Users>()
                .Find(user => user.PhoneNumber == phoneNumber)
                .Project(user => user.PhoneNumber)
                .AnyAsync();
        }
        #endregion

        #region Login
        public async Task<AuthenticationTokenDto> LoginAsync(LoginDto loginUser)
        {
            string email = loginUser.Email;
            string password = loginUser.Password;

            // Kiểm tra tài khoản có tồn tại hay không
            Users user = await _unitOfWork.GetCollection<Users>()
                .Find(user => user.Email == email)
                .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Account does not exist");

            // Kiểm tra mật khẩu có đúng hay không
            bool isPasswordValid = VerifyPassword(password, user.Password);
            if (!isPasswordValid)
            {
                throw new BadRequestCustomException("Invalid password");
            }

            // Tạo token cho người dùng
            IEnumerable<Claim> claims =
            [
                new Claim("Id", user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.StreetAddress, user.Address),
                new Claim("Image", user.Image),
                new Claim(ClaimTypes.Role, user.Role)
            ];

            // Generate access token
            string accessToken = _jsonWebToken.GenerateAccessToken(claims);

            AuthenticationTokenDto authenticationTokenDto = new()
            {
                AccessToken = accessToken,
                Id = user.Id,
                FullName = user.FullName,
                Role = user.Role,
                Image = user.Image
                //RefreshToken = null, // Refresh token is not implemented yet
                //ExpiresIn = 30 * 24 * 60 * 60 // Token expiration time in seconds (30 days)
            };

            return authenticationTokenDto;
        }
        #endregion

        #region ChangePassword
        public async Task ChangePasswordAsync(string newPassword, string oldPassword)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedCustomException("Your session is limit, you must login again to edit profile!");
            }

            // Kiểm tra tài khoản có tồn tại hay không
            Users user = await _unitOfWork.GetCollection<Users>()
                .Find(user => user.Id == userId)
                .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Account does not exist");

            // Kiểm tra mật khẩu cũ có đúng hay không
            bool isOldPasswordValid = VerifyPassword(oldPassword, user.Password);
            if (!isOldPasswordValid)
            {
                throw new BadRequestCustomException("Invalid old password");
            }

            // Mã hóa mật khẩu mới
            string hashedNewPassword = HashPassword(newPassword);

            // Cập nhật mật khẩu mới cho người dùng
            var updateDefinition = Builders<Users>.Update.Set(user => user.Password, hashedNewPassword);
            await _unitOfWork.GetCollection<Users>().UpdateOneAsync(user => user.Id == userId, updateDefinition);
        }
        #endregion
    }
}
