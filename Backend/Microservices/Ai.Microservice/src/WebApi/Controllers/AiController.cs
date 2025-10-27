using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.AiWebhook.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using SharedLibrary.Attributes;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize("Admin", "Staff")]
public class AiController : ApiController
{
    public AiController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
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
}

public class UploadAiRequest
{
    [Required]
    public IFormFile? File { get; set; }
}
