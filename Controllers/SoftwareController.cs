using Bugtrack.Api.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/software")]
public class SoftwareController : ControllerBase
{
    private readonly ISoftwareService softwareService;
    private readonly IProjectService projectService;

    public SoftwareController(
        ISoftwareService softwareService,
        IProjectService projectService)
    {
        this.softwareService = softwareService;
        this.projectService = projectService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid projectId,
        CreateSoftwareRequest data)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(id, out var userId))
        {
            return Unauthorized();
        }

        var project = await projectService.GetByIdAsync(userId, projectId);

        if (project == null)
        {
            return NotFound();
        }

        if (project.Role != ProjectRole.Manager)
        {
            return Forbid();
        }

        var software = await softwareService.CreateAsync(projectId, data);

        return StatusCode(201, software);
    }

    [HttpGet]
    public async Task<IActionResult> GetByProject(Guid projectId)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(id, out var userId))
        {
            return Unauthorized();
        }

        var project = await projectService.GetByIdAsync(userId, projectId);

        if (project == null)
        {
            return NotFound();
        }

        var software = await softwareService.GetByProjectAsync(projectId);

        return Ok(software);
    }
}