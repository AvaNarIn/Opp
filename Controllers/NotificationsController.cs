using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService notificationService;

    public NotificationsController(
        INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    [HttpGet]
    public Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery(Name = "per_page")] int perPage = 10,
        [FromQuery(Name = "is_read")] bool? isRead = null)
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

            var result = await notificationService.GetAsync(
                GetUserId(),
                page,
                perPage,
                isRead);

            return Ok(result);
        });
    }

    [HttpGet("unread-count")]
    public Task<IActionResult> GetUnreadCount()
    {
        return ExecuteAsync(async () =>
        {
            var count = await notificationService.GetUnreadCountAsync(
                GetUserId());

            return Ok(new { count });
        });
    }

    [HttpPatch("{notificationId:guid}/read")]
    public Task<IActionResult> MarkRead(Guid notificationId)
    {
        return ExecuteAsync(async () =>
        {
            await notificationService.MarkReadAsync(
                GetUserId(),
                notificationId);

            return NoContent();
        });
    }

    [HttpPatch("read-all")]
    public Task<IActionResult> MarkAllRead()
    {
        return ExecuteAsync(async () =>
        {
            await notificationService.MarkAllReadAsync(GetUserId());

            return NoContent();
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