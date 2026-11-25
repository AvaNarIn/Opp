public interface IAuthService
{
    Task<Guid?> RegisterAsync(RegisterRequest data);
    Task<LoginResponse?> LoginAsync(LoginRequest data);
    Task<UserResponse?> GetCurrentUserAsync(Guid userId);
}