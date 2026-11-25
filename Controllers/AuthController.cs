using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest data)
    {
        var id = await authService.RegisterAsync(data);

        if (id == null)
        {
            return Conflict(new
            {
                email = "already exists"
            });
        }

        return StatusCode(201, new { id = id.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest data)
    {
        var result = await authService.LoginAsync(data);

        if (result == null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var session = User.FindFirst("session")?.Value;

        if (!Guid.TryParse(id, out var userId) ||
            string.IsNullOrEmpty(session))
        {
            return Unauthorized();
        }

        var user = await authService.GetCurrentUserAsync(userId);

        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }
}