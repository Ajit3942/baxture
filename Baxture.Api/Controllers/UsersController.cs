using Baxture.Api.Attributes;
using Baxture.Api.Dtos;
using Baxture.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Baxture.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AuthorizeUser]
public sealed class UsersController(IUserService userService, IUserExportService exportService) : ControllerBase
{
    [HttpGet]
    [AdminOnly]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users.Select(UserResponse.FromUser));
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById(string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out _))
        {
            return BadRequest(new { message = "userId must be a valid UUID." });
        }

        var user = await userService.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? NotFound(new { message = $"User with id '{userId}' was not found." })
            : Ok(UserResponse.FromUser(user));
    }

    [HttpPost]
    [AdminOnly]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var validation = UserRequestValidator.ValidateCreate(request);
        if (validation.Count > 0)
        {
            return BadRequest(new { message = "Request body is invalid.", errors = validation });
        }

        var created = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { userId = created.Id }, UserResponse.FromUser(created));
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(string userId, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out _))
        {
            return BadRequest(new { message = "userId must be a valid UUID." });
        }

        var currentUser = HttpContext.GetCurrentUser()!;
        if (!currentUser.IsAdmin && currentUser.Id != userId)
        {
            return new ObjectResult(new { message = "Normal users cannot update another user's data." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        var validation = UserRequestValidator.ValidateUpdate(request);
        if (validation.Count > 0)
        {
            return BadRequest(new { message = "Request body is invalid.", errors = validation });
        }

        var updated = await userService.UpdateAsync(userId, request, cancellationToken);
        return updated is null
            ? NotFound(new { message = $"User with id '{userId}' was not found." })
            : Ok(UserResponse.FromUser(updated));
    }

    [HttpDelete("{userId}")]
    [AdminOnly]
    public async Task<IActionResult> Delete(string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out _))
        {
            return BadRequest(new { message = "userId must be a valid UUID." });
        }

        var deleted = await userService.DeleteAsync(userId, cancellationToken);
        return deleted
            ? NoContent()
            : NotFound(new { message = $"User with id '{userId}' was not found." });
    }

    [HttpPost("search")]
    [AdminOnly]
    public async Task<IActionResult> Search([FromBody] SearchUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("export")]
    [AdminOnly]
    public async Task<IActionResult> Export([FromBody] ExportUsersRequest request, CancellationToken cancellationToken)
    {
        if (!request.Format.Equals("pdf", StringComparison.OrdinalIgnoreCase) &&
            !request.Format.Equals("excel", StringComparison.OrdinalIgnoreCase) &&
            !request.Format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Export format must be PDF or EXCEL." });
        }

        var result = await userService.SearchAsync(request.Search, cancellationToken);
        var export = exportService.Export(result.Items, request.Format);
        return File(export.Content, export.ContentType, export.FileName);
    }
}
