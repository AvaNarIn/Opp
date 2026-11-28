using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/tickets/{ticketId:guid}/attachments")]
public class AttachmentFilesController : ControllerBase
{
    private readonly IAttachmentFileService fileService;

    public AttachmentFilesController(
        IAttachmentFileService fileService)
    {
        this.fileService = fileService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    public Task<IActionResult> Upload(
        Guid projectId,
        Guid ticketId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(async () =>
        {
            var attachment = await fileService.UploadAsync(
                GetUserId(),
                projectId,
                ticketId,
                file,
                cancellationToken);

            return StatusCode(201, attachment);
        });
    }

    [HttpGet("{attachmentId:guid}/download")]
    public Task<IActionResult> Download(
        Guid projectId,
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken){
        return ExecuteAsync(async () =>
        {
            var download = await fileService.DownloadAsync(
                GetUserId(),
                projectId,
                ticketId,
                attachmentId,
                cancellationToken);

            return File(
                download.Content,
                download.ContentType,
                download.FileName,
                enableRangeProcessing: true);
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