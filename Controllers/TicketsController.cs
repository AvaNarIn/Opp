using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService ticketService;

    public TicketsController(ITicketService ticketService)
    {
        this.ticketService = ticketService;
    }

    [HttpPost]
    public Task<IActionResult> Create(
        Guid projectId,
        CreateTicketRequest data)
    {
        return ExecuteAsync(async () =>
        {
            var ticket = await ticketService.CreateAsync(
                GetUserId(),
                projectId,
                data);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    projectId,
                    ticketId = ticket.Id
                },
                ticket);
        });
    }

    [HttpGet]
    public Task<IActionResult> GetByProject(
        Guid projectId,
        [FromQuery] TicketQueryRequest query)
    {
        return ExecuteAsync(async () =>
        {
            var tickets = await ticketService.GetByProjectAsync(
                GetUserId(),
                projectId,
                query);

            return Ok(tickets);
        });
    }

    [HttpGet("{ticketId:guid}")]
    public Task<IActionResult> GetById(
        Guid projectId,
        Guid ticketId)
    {
        return ExecuteAsync(async () =>
        {
            var ticket = await ticketService.GetByIdAsync(
                GetUserId(),
                projectId,
                ticketId);

            return Ok(ticket);
        });
    }

    [HttpPut("{ticketId:guid}")]
    public Task<IActionResult> Update(
        Guid projectId,
        Guid ticketId,
        UpdateTicketRequest data)
    {
        return ExecuteAsync(async () =>
        {
            var ticket = await ticketService.UpdateAsync(
                GetUserId(),
                projectId,
                ticketId,
                data);

            return Ok(ticket);
        });
    }

    [HttpPatch("{ticketId:guid}/plan")]
    public Task<IActionResult> Plan(
        Guid projectId,
        Guid ticketId,
        PlanTicketRequest data)
    {
        return ExecuteAsync(async () =>
        {
            var ticket = await ticketService.PlanAsync(
                GetUserId(),
                projectId,
                ticketId,
                data);

            return Ok(ticket);
        });
    }

    [HttpPatch("{ticketId:guid}/status")]
    public Task<IActionResult> ChangeStatus(
        Guid projectId,
        Guid ticketId,
        ChangeTicketStatusRequest data)
    {
        return ExecuteAsync(async () =>
        {
            var ticket = await ticketService.ChangeStatusAsync(
                GetUserId(),
                projectId,
                ticketId,
                data);

            return Ok(ticket);
        });
    }

    [HttpPost("{ticketId:guid}/archive")]
    public Task<IActionResult> Archive(
        Guid projectId,
        Guid ticketId)
    {
        return ExecuteAsync(async () =>
        {
            await ticketService.ArchiveAsync(
                GetUserId(),
                projectId,
                ticketId);

            return NoContent();
        });
    }

    [HttpPost("{ticketId:guid}/restore")]
    public Task<IActionResult> Restore(
        Guid projectId,
        Guid ticketId)
    {
        return ExecuteAsync(async () =>
        {
            await ticketService.RestoreAsync(
                GetUserId(),
                projectId,
                ticketId);

            return NoContent();
        });
    }

    [HttpGet("{ticketId:guid}/history")]
    public Task<IActionResult> GetHistory(
        Guid projectId,
        Guid ticketId)
    {
        return ExecuteAsync(async () =>
        {
            var history = await ticketService.GetHistoryAsync(
                GetUserId(),
                projectId,
                ticketId);

            return Ok(history);
        });
    }

    [HttpGet("{ticketId:guid}/assignments")]
    public Task<IActionResult> GetAssignments(
        Guid projectId,
        Guid ticketId)
    {
        return ExecuteAsync(async () =>
        {
            var assignments = await ticketService.GetAssignmentsAsync(
                GetUserId(),
                projectId,
                ticketId);

            return Ok(assignments);
        });
    }

    [HttpGet("{ticketId:guid}/attachments")]
    public Task<IActionResult> GetAttachments(
        Guid projectId,
        Guid ticketId)
    {
        return ExecuteAsync(async () =>
        {
            var attachments = await ticketService.GetAttachmentsAsync(
                GetUserId(),
                projectId,
                ticketId);

            return Ok(attachments);
        });
    }

    [HttpPost("{ticketId:guid}/attachments")]
    public Task<IActionResult> AddAttachment(
        Guid projectId,
        Guid ticketId,
        AddTicketAttachmentRequest data)
    {
        return ExecuteAsync(async () =>
        {
            var attachment = await ticketService.AddAttachmentAsync(
                GetUserId(),
                projectId,
                ticketId,
                data);

            return StatusCode(201, attachment);
        });
    }

    [HttpDelete("{ticketId:guid}/attachments/{attachmentId:guid}")]
    public Task<IActionResult> RemoveAttachment(
        Guid projectId,
        Guid ticketId,
        Guid attachmentId)
    {
        return ExecuteAsync(async () =>
        {
            await ticketService.RemoveAttachmentAsync(
                GetUserId(),
                projectId,
                ticketId,
                attachmentId);

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