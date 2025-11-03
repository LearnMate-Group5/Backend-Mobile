using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Subscriptions;
using Application.Subscriptions.Commands;
using Application.Subscriptions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/subscription/plans")]
public sealed class SubscriptionPlansController : ApiController
{
    public SubscriptionPlansController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSubscriptionPlansQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{subscriptionId:guid}", Name = "GetSubscriptionPlanById")]
    [Authorize("Admin", "Staff", "User")]
    public async Task<IActionResult> GetSubscriptionPlanByIdAsync(
        [FromRoute] Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSubscriptionPlanByIdQuery(subscriptionId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Subscription.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> CreateSubscriptionPlanAsync(
        [FromBody] CreateSubscriptionPlanRequest? request,
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

        var command = new CreateSubscriptionPlanCommand(
            request.Name,
            request.Type,
            request.Status,
            request.OriginalPrice,
            request.Discount);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Subscription.DuplicateNameAndType")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return CreatedAtRoute(
            "GetSubscriptionPlanById",
            new { subscriptionId = result.Value.SubscriptionId },
            result.Value);
    }

    [HttpPut("{subscriptionId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> UpdateSubscriptionPlanAsync(
        [FromRoute] Guid subscriptionId,
        [FromBody] UpdateSubscriptionPlanRequest? request,
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

        var command = new UpdateSubscriptionPlanCommand(
            subscriptionId,
            request.Name,
            request.Type,
            request.Status,
            request.OriginalPrice,
            request.Discount);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Subscription.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            if (result.Error.Code == "Subscription.DuplicateNameAndType")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{subscriptionId:guid}")]
    [Authorize("Admin", "Staff")]
    public async Task<IActionResult> DeleteSubscriptionPlanAsync(
        [FromRoute] Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteSubscriptionPlanCommand(subscriptionId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Subscription.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(new { message = "Subscription plan deleted successfully." });
    }

    [HttpGet("my")]
    [Authorize("User")]
    public async Task<IActionResult> GetMySubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication is required." });
        }

        var result = await _mediator.Send(new GetUserSubscriptionsQuery(userId), cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("my/current")]
    [Authorize("User")]
    public async Task<IActionResult> GetMyCurrentSubscriptionAsync(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication is required." });
        }

        var result = await _mediator.Send(new GetActiveUserSubscriptionQuery(userId), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "UserSubscription.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("{subscriptionId:guid}/upgrade")]
    [Authorize("User")]
    public async Task<IActionResult> UpgradeSubscriptionAsync(
        [FromRoute] Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication is required." });
        }

        var command = new UpdateUserSubscriptionPlanCommand(subscriptionId, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Subscription.NotFound" => NotFound(new { message = result.Error.Description }),
                "UserSubscription.NotFound" => NotFound(new { message = result.Error.Description }),
                "Subscription.Inactive" => BadRequest(new { message = result.Error.Description }),
                "UserSubscription.Inactive" => Conflict(new { message = result.Error.Description }),
                "Subscription.UpgradeRequiresHigherPrice" => BadRequest(new { message = result.Error.Description }),
                "Subscription.AlreadyCurrent" => BadRequest(new { message = result.Error.Description }),
                _ => HandleFailure(result)
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("{subscriptionId:guid}/choose")]
    [Authorize("User")]
    public async Task<IActionResult> ChooseSubscriptionPlanAsync(
        [FromRoute] Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication is required." });
        }

        var command = new ChooseSubscriptionPlanCommand(subscriptionId, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Subscription.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            if (result.Error.Code == "Subscription.Inactive")
            {
                return BadRequest(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return CreatedAtRoute(
            "GetSubscriptionPlanById",
            new { subscriptionId = result.Value.SubscriptionId },
            result.Value);
    }

    [HttpPost("cancel")]
    [Authorize("User")]
    public async Task<IActionResult> CancelSubscriptionAsync(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication is required." });
        }

        var command = new CancelUserSubscriptionCommand(userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "UserSubscription.NotFound")
            {
                return NotFound(new { message = result.Error.Description });
            }

            if (result.Error.Code == "UserSubscription.AlreadyCancelled")
            {
                return Conflict(new { message = result.Error.Description });
            }

            return HandleFailure(result);
        }

        return Ok(result.Value);
    }
}

public sealed class CreateSubscriptionPlanRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Status { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal OriginalPrice { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal Discount { get; set; }
}

public sealed class UpdateSubscriptionPlanRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(100)]
    public string? Type { get; set; }

    [MaxLength(100)]
    public string? Status { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal? OriginalPrice { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? Discount { get; set; }
}
