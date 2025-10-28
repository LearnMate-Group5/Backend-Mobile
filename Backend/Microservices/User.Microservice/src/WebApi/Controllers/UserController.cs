using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Application.Users.Commands;
using Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using SharedLibrary.Authentication;
using SharedLibrary.Attributes;
using SharedLibrary.Common.Commands;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ApiController
    {
        public UserController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }
            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
        }
        return Ok(result);
    }

        [HttpPost("login/firebase")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithFirebase([FromBody] LoginWithFirebaseRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginWithFirebaseCommand(request.IdToken);
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }
            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
            }
            return Ok(result);
        }

        [HttpPut("role")]
        [Authorize("Admin")]
        public async Task<IActionResult> UpdateRole([FromQuery] Guid userId, [FromQuery] string roleName, CancellationToken cancellationToken)
        {
            var command = new UpdateUserRoleCommand(userId, roleName);
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
            }

            return Ok(result);
        }

        [HttpPut("{userId:guid}/activation")]
        [Authorize("Admin")]
        public async Task<IActionResult> UpdateActivationStatus([FromRoute] Guid userId, [FromQuery] bool isActive, CancellationToken cancellationToken)
        {
            var command = new UpdateUserActivationStatusCommand(userId, isActive);
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
            }

            return Ok(result);
        }

        [HttpPut("{userId:guid}/profile")]
        [Authorize("Admin", "User")]
        public async Task<IActionResult> UpdateProfile([FromRoute] Guid userId, [FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateUserProfileCommand(userId, request.Name, request.Email, request.AvatarUrl);
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
            }

            return NoContent();
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }
            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
            }
            return Ok(result);
        }

        [HttpGet("read")]
        [Authorize("Admin", "User")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var query = new GetAllUsersQuery(pageNumber, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("roles/me")]
        [Authorize("Admin", "Staff", "User")]
        public async Task<IActionResult> GetCurrentUserRoles(CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("User context is missing.");
            }

            var result = await _mediator.Send(new GetUserRolesQuery(userId), cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok(result.Value);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok();
        }
    }

    public sealed record UpdateUserProfileRequest(string Name, string Email, string? AvatarUrl);
    public sealed record LoginWithFirebaseRequest(string IdToken);
}
