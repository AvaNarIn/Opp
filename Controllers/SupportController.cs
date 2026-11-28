using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Authorize]
[Route("api/support")]
public class SupportController : ControllerBase
{
    private readonly ISupportService supportService;

    public SupportController(ISupportService supportService)
    {
        this.supportService = supportService;
    }

    [HttpPost]
    public Task<IActionResult> Create(CreateSupportRequest data)
    {
        return ExecuteAsync(async () =>
        {
            var message = await supportService.CreateAsync(
                GetUserId(),
                data);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    messageId = message.Id
                },
                message);
        });
    }

    [HttpGet]
    public Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery(Name = "per_page")] int perPage = 10)
    {
        return ExecuteAsync(async () =>
        {
            if (page < 1 || perPage < 1 || perPage > 100)
            {
                return BadRequest(new
                {
                    message = "Номер страницы должен быть положительным, размер — от 1 до 100."
                });
            }

            var result = await supportService.GetAsync(
                GetUserId(),
                page,
                perPage);

            return Ok(result);
        });
    }

    [HttpGet("{messageId:guid}")]
    public Task<IActionResult> GetById(Guid messageId)
    {
        return ExecuteAsync(async () =>
        {
            var message = await supportService.GetByIdAsync(
                GetUserId(),
                messageId);

            return Ok(message);
        });
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(id, out var userId))
        {
            throw new TicketOperationException(
                401,
                "Требуется авторизация.");
        }

        return userId;
    }

    private async Task<IActionResult> ExecuteAsync(
        Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (TicketOperationException exception)
        {
            return StatusCode(
                exception.StatusCode,
                new
                {
                    message = exception.Message
                });
        }
    }
}