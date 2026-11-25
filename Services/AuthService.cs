using Bugtrack.Api.Classes;
using Bugtrack.Api.Data;
using Bugtrack.Api.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class AuthService : IAuthService
{
    private readonly AppDbContext db;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly ITokenService tokenService;

    public AuthService(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        this.db = db;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
    }

    public async Task<Guid?> RegisterAsync(RegisterRequest data)
    {
        var email = data.Email.Trim().ToLowerInvariant();

        var emailExists = await db.Users.AnyAsync(
            user => user.Email == email);

        if (emailExists)
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = data.Name.Trim(),
            Email = email,
            SystemRole = SystemRole.User
        };

        user.PasswordHash = passwordHasher.HashPassword(
            user,
            data.Password);

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            if (exception.InnerException is PostgresException postgres &&
                postgres.SqlState == PostgresErrorCodes.UniqueViolation &&
                postgres.ConstraintName == "UX_users_Email")
            {
                db.Entry(user).State = EntityState.Detached;
                return null;
            }

            throw;
        }

        return user.Id;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest data)
    {
        if (string.IsNullOrWhiteSpace(data.Email) ||
            string.IsNullOrEmpty(data.Password))
        {
            return null;
        }

        var email = data.Email.Trim().ToLowerInvariant();

        var user = await db.Users.SingleOrDefaultAsync(
            user => user.Email == email);

        if (user == null)
        {
            return null;
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            data.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return tokenService.Create(user);
    }

    public async Task<UserResponse?> GetCurrentUserAsync(Guid userId)
    {
        return await db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                SystemRole = user.SystemRole
            })
            .SingleOrDefaultAsync();
    }
}