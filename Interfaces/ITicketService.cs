public interface ITicketService
{
    Task<TicketResponse> CreateAsync(Guid userId,  Guid projectId, CreateTicketRequest data);

    Task<PagedTicketsResponse> GetByProjectAsync(Guid userId, Guid projectId, TicketQueryRequest query);

    Task<TicketResponse> GetByIdAsync( Guid userId, Guid projectId, Guid ticketId);

    Task<TicketResponse> UpdateAsync( Guid userId, Guid projectId, Guid ticketId,  UpdateTicketRequest data);

    Task<TicketResponse> PlanAsync(Guid userId,Guid projectId,Guid ticketId, PlanTicketRequest data);

    Task<TicketResponse> ChangeStatusAsync(Guid userId,Guid projectId, Guid ticketId, ChangeTicketStatusRequest data);

    Task ArchiveAsync(Guid userId,Guid projectId,Guid ticketId);

    Task RestoreAsync( Guid userId, Guid projectId,Guid ticketId);

    Task<List<TicketStatusHistoryResponse>> GetHistoryAsync( Guid userId,Guid projectId,Guid ticketId);

    Task<List<TicketAssignmentResponse>> GetAssignmentsAsync(Guid userId,Guid projectId, Guid ticketId);

    Task<List<TicketAttachmentResponse>> GetAttachmentsAsync(Guid userId,Guid projectId, Guid ticketId);

    Task<TicketAttachmentResponse> AddAttachmentAsync(Guid userId, Guid projectId, Guid ticketId,  AddTicketAttachmentRequest data);

    Task RemoveAttachmentAsync(Guid userId, Guid projectId, Guid ticketId, Guid attachmentId);
}