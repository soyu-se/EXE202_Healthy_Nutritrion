namespace HealthyNutritionApp.Application.Dto.Authentication
{
    public class AuthenticationTokenDto
    {
        public string AccessToken { get; set; }
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Image { get; set; }
    }
}
