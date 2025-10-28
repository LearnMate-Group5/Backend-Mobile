using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using Application.AiWebhook.Commands;
using Application.AiWebhook.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using SharedLibrary.Attributes;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ApiController
{
    public AiController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> UploadAsync(
        [FromForm] UploadAiRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User context is missing.");
        }

        await using var stream = request.File.OpenReadStream();

        var command = new UploadAndTranslateCommand(
            stream,
            request.File.FileName,
            request.File.ContentType ?? "application/octet-stream",
            userId);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("file")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetFilesAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User context is missing.");
        }

        var roleSet = User.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var includeAll = roleSet.Contains("Admin") || roleSet.Contains("Staff");

        var query = new GetAiFilesQuery(userId, includeAll);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("session")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetSessionsAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User context is missing.");
        }

        var roleSet = User.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var includeAll = roleSet.Contains("Admin") || roleSet.Contains("Staff");

        var result = await _mediator.Send(new GetAiSessionsQuery(userId, includeAll), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("file/{fileId:guid}")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetFileByIdAsync(
        [FromRoute] Guid fileId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User context is missing.");
        }

        var result = await _mediator.Send(new GetAiFileByIdQuery(fileId), cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        var roleSet = User.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var includeAll = roleSet.Contains("Admin") || roleSet.Contains("Staff");
        if (!includeAll && !result.Value.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        return Ok(result.Value);
    }

    [HttpPut("file/{fileId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> UpdateFileAsync(
        [FromRoute] Guid fileId,
        [FromBody] UpdateAiFileRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var command = new UpdateAiFileCommand(
            fileId,
            request.FileName,
            request.OcrContent,
            request.TranslatedContent,
            request.CurrentContent);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("file/{fileId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> DeleteFileAsync(
        [FromRoute] Guid fileId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteAiFileCommand(fileId), cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(new { message = "File deleted successfully." });
    }
}

public class UploadAiRequest
{
    [Required]
    public IFormFile? File { get; set; }
}

public sealed class UpdateAiFileRequest
{
    [MaxLength(255)]
    public string? FileName { get; set; }

    public string? OcrContent { get; set; }

    public string? TranslatedContent { get; set; }

    public string? CurrentContent { get; set; }
}
