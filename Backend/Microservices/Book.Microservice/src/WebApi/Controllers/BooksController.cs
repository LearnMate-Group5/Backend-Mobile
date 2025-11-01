using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Books;
using Application.Books.Commands;
using Application.Books.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/Book")]
public sealed class BooksController : ApiController
{
    public BooksController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetBooksAsync(
        [FromQuery] bool onlyActive = false,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? categoryName = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBooksQuery(onlyActive, categoryId, categoryName);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{bookId:guid}", Name = "GetBookById")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetBookByIdAsync(
        [FromRoute] Guid bookId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBookByIdQuery(bookId), cancellationToken);

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

    [HttpPost]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> CreateBookAsync(
        [FromForm] CreateBookRequest? request,
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

        var imageBase64 = await ReadFileAsBase64Async(request.ImageFile, cancellationToken);

        var command = new CreateBookCommand(
            request.Title,
            request.Author,
            request.Description,
            imageBase64,
            request.Categories ?? new List<string>(),
            userId);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Book.DuplicateTitle")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return CreatedAtRoute("GetBookById", new { bookId = result.Value.BookId }, result.Value);
    }

    [HttpPut("{bookId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> UpdateBookAsync(
        [FromRoute] Guid bookId,
        [FromForm] UpdateBookRequest? request,
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

        var imageBase64 = await ReadFileAsBase64Async(request.ImageFile, cancellationToken);

        var command = new UpdateBookCommand(
            bookId,
            request.Title,
            request.Author,
            request.Description,
            imageBase64,
            request.Categories,
            request.IsActive,
            userId,
            request.ClearImage);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Book.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            if (result.Error.Code == "Book.DuplicateTitle")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{bookId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> DeleteBookAsync(
        [FromRoute] Guid bookId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteBookCommand(bookId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Book.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(new { message = "Book deleted successfully." });
    }

    private static async Task<string?> ReadFileAsBase64Async(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        return Convert.ToBase64String(memoryStream.ToArray());
    }
}

public sealed class CreateBookRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    public IFormFile? ImageFile { get; set; }

    [Required]
    [MinLength(1)]
    public List<string> Categories { get; set; } = new();
}

public sealed class UpdateBookRequest
{
    [DefaultValue(null)]
    [MaxLength(200)]
    public string? Title { get; set; }

    [DefaultValue(null)]
    [MaxLength(200)]
    public string? Author { get; set; }

    [DefaultValue(null)]
    [MaxLength(4000)]
    public string? Description { get; set; }

    public IFormFile? ImageFile { get; set; }

    [DefaultValue(null)]
    public List<string>? Categories { get; set; }

    public bool? IsActive { get; set; }

    [DefaultValue(false)]
    public bool ClearImage { get; set; } = false;
}









