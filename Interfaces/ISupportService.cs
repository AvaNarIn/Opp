public interface ISupportService
{
    Task<SupportResponse> CreateAsync(Guid userId, CreateSupportRequest data);

    Task<SupportMessagesResponseDto> GetAsync(Guid userId, int page, int perPage);

    Task<SupportResponse> GetByIdAsync(Guid userId, Guid messageId);
}