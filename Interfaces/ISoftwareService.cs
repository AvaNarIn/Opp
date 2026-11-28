public interface ISoftwareService
{
    Task<SoftwareResponse> CreateAsync(Guid projectId, CreateSoftwareRequest data);

    Task<List<SoftwareResponse>> GetByProjectAsync(Guid projectId);

    Task<SoftwareResponse> UpdateAsync(Guid userId, Guid projectId, Guid softwareId, UpdateSoftwareRequest data);

    Task DeleteAsync(Guid userId, Guid projectId, Guid softwareId);
}