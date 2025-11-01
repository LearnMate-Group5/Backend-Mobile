using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Application.Categories;
using Application.Categories.Commands;
using Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/book/Categories")]
public sealed class CategoriesController : ApiController
{
    public CategoriesController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{categoryId:guid}", Name = "GetCategoryById")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetCategoryByIdAsync(
        [FromRoute] Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(categoryId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Category.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{categoryId:guid}/book-names")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetBookNamesByCategoryAsync(
        [FromRoute] Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBookNamesByCategoryIdQuery(categoryId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Category.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }
    [HttpPost]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> CreateCategoryAsync(
        [FromBody] CreateCategoryRequest? request,
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

        var result = await _mediator.Send(new CreateCategoryCommand(request.Name), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Category.DuplicateName")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return CreatedAtRoute("GetCategoryById", new { categoryId = result.Value.CategoryId }, result.Value);
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> UpdateCategoryAsync(
        [FromRoute] Guid categoryId,
        [FromBody] UpdateCategoryRequest? request,
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

        var command = new UpdateCategoryCommand(categoryId, request.Name);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Category.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            if (result.Error.Code == "Category.DuplicateName")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{categoryId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> DeleteCategoryAsync(
        [FromRoute] Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(categoryId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Category.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(new { message = "Category deleted successfully." });
    }
}

public sealed class CreateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
