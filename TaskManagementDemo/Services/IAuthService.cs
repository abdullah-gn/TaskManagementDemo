namespace TaskManagementDemo.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(string userId, string email);
    }
}
