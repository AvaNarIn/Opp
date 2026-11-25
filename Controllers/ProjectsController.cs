using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Bugtrack.Api.Enums;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService projectService;

    public ProjectsController(IProjectService projectService)
    {
        this.projectService = projectService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectRequest data)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(id, out var userId))
        {
            return Unauthorized();
        }

        var projectId = await projectService.CreateAsync(userId, data);
        if (projectId == null)
        {
            return Unauthorized();
        }
        return StatusCode(201, new { id = projectId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(id, out var userId))
        {
            return Unauthorized();
        }

        var projects = await projectService.GetMyProjectsAsync(userId);
        return Ok(projects);
    }


    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetById(Guid projectId)
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
        return Ok(project);
    }

    [HttpGet("{projectId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid projectId)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(id, out var userId))
        {
            return Unauthorized();
        }

        var members = await projectService.GetMembersAsync(userId, projectId);
        if (members == null)
        {
            return NotFound();
        }
        return Ok(members);
    }


    [HttpPost("{projectId:guid}/members")]
    public async Task<IActionResult> AddMember(
        Guid projectId,
        AddProjectMemberRequest data)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(id, out var userId))
        {
            return Unauthorized();
        }

        var result = await projectService.AddMemberAsync(
            userId,
            projectId,
            data);

        switch (result)
        {
            case AddProjectMemberResult.Success:
                return NoContent();

            case AddProjectMemberResult.ProjectNotFound:
                return NotFound();

            case AddProjectMemberResult.Forbidden:
                return Forbid();

            case AddProjectMemberResult.UserNotFound:
                return NotFound(new
                {
                    message = "Пользователь с такой почтой не найден."
                });

            case AddProjectMemberResult.AlreadyMember:
                return Conflict(new
                {
                    message = "Пользователь уже состоит в проекте."
                });

            case AddProjectMemberResult.InvalidRole:
                return BadRequest(new
                {
                    message = "Некорректная роль."
                });

            default:
                return StatusCode(500);
        }
    }

    [HttpPatch("{projectId:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> ChangeMemberRole(
        Guid projectId,
        Guid memberId,
        ChangeProjectMemberRoleRequest data)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(id, out var userId))
        {
            return Unauthorized();
        }

        var result = await projectService.ChangeMemberRoleAsync(
            userId,
            projectId,
            memberId,
            data);

        return MemberChangeResponse(result);
    }

    [HttpDelete("{projectId:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(
        Guid projectId,
        Guid memberId)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(id, out var userId))
        {
            return Unauthorized();
        }

        var result = await projectService.RemoveMemberAsync(
            userId,
            projectId,
            memberId);

        return MemberChangeResponse(result);
    }

    private IActionResult MemberChangeResponse(
        ChangeProjectMemberResult result)
    {
        switch (result)
        {
            case ChangeProjectMemberResult.Success:
                return NoContent();

            case ChangeProjectMemberResult.ProjectNotFound:
                return NotFound();

            case ChangeProjectMemberResult.Forbidden:
                return Forbid();

            case ChangeProjectMemberResult.MemberNotFound:
                return NotFound(new
                {
                    message = "Участник не найден в проекте."
                });

            case ChangeProjectMemberResult.LastManager:
                return Conflict(new
                {
                    message = "В проекте должен остаться хотя бы один менеджер."
                });

            case ChangeProjectMemberResult.InvalidRole:
                return BadRequest(new
                {
                    message = "Некорректная роль."
                });

            default:
                return StatusCode(500);
        }
    }

    
}