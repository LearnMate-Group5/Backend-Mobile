using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.BookChapters;
using Application.BookChapters.Commands;
using Application.BookChapters.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/Book/{bookId:guid}/chapters")]
public sealed class BookChaptersController : ApiController
{
    public BookChaptersController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetChaptersAsync(
        [FromRoute] Guid bookId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBookChaptersQuery(bookId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Book.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{chapterId:guid}", Name = "GetBookChapterById")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetChapterByIdAsync(
        [FromRoute] Guid bookId,
        [FromRoute] Guid chapterId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBookChapterByIdQuery(bookId, chapterId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Book.NotFound" || result.Error.Code == "BookChapter.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> CreateChapterAsync(
        [FromRoute] Guid bookId,
        [FromBody] CreateBookChapterRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User context is missing.");
        }

        var command = new CreateBookChapterCommand(
            bookId,
            request.PageIndex,
            request.Title,
            request.Content,
            userId);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Book.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            if (result.Error.Code == "BookChapter.DuplicatePageIndex")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return CreatedAtRoute(
            "GetBookChapterById",
            new { bookId, chapterId = result.Value.ChapterId },
            result.Value);
    }

    [HttpPut("{chapterId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> UpdateChapterAsync(
        [FromRoute] Guid bookId,
        [FromRoute] Guid chapterId,
        [FromBody] UpdateBookChapterRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User context is missing.");
        }

        var command = new UpdateBookChapterCommand(
            bookId,
            chapterId,
            request.PageIndex,
            request.Title,
            request.Content,
            userId);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Book.NotFound" || result.Error.Code == "BookChapter.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            if (result.Error.Code == "BookChapter.DuplicatePageIndex")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{chapterId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> DeleteChapterAsync(
        [FromRoute] Guid bookId,
        [FromRoute] Guid chapterId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteBookChapterCommand(bookId, chapterId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "BookChapter.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(new { message = "Chapter deleted successfully." });
    }
}

public sealed class CreateBookChapterRequest
{
    [Range(0, int.MaxValue)]
    public int PageIndex { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(8000)]
    public string Content { get; set; } = string.Empty;
}

public sealed class UpdateBookChapterRequest
{
    [Range(0, int.MaxValue)]
    public int PageIndex { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(8000)]
    public string Content { get; set; } = string.Empty;
}
