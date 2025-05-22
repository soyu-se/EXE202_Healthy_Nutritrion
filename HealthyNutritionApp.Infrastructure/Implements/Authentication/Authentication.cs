using BCrypt.Net;

namespace HealthyNutritionApp.Infrastructure.Implements.Authentication
{
    public class Authentication
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }


    }
}
