public interface IAttachmentFileService
{
    Task<TicketAttachmentResponse> UploadAsync(Guid userId,Guid projectId,Guid ticketId,IFormFile file,CancellationToken cancellationToken);

    Task<AttachmentDownload> DownloadAsync(Guid userId,Guid projectId,Guid ticketId,Guid attachmentId,CancellationToken cancellationToken);
}