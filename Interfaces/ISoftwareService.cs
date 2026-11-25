public interface ISoftwareService
{
    Task<SoftwareResponse> CreateAsync(Guid projectId, CreateSoftwareRequest data);

    Task<List<SoftwareResponse>> GetByProjectAsync(Guid projectId);
}