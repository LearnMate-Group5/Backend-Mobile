using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IO;
using Application.Users.Commands;
using Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Common;
using SharedLibrary.Authentication;
using SharedLibrary.Attributes;
using SharedLibrary.Common.Commands;
using Microsoft.AspNetCore.Http;

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
        public async Task<IActionResult> UpdateActivationStatus([FromRoute] Guid userId, [FromQuery] bool? isActive, CancellationToken cancellationToken)
        {
            var command = new UpdateUserActivationStatusCommand(userId, isActive ?? true);
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

        [HttpPut("password")]
        [HttpPut("users/password")]
        [Authorize("Admin", "Staff", "User")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("User context is missing.");
            }

            var command = new ChangePasswordCommand(
                userId,
                request.OldPassword ?? string.Empty,
                request.NewPassword ?? string.Empty,
                request.ConfirmNewPassword ?? string.Empty);

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

        [HttpPost("users/password/forgot")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email is required.");
            }

            var command = new RequestPasswordResetCommand(request.Email);
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

            return Ok(new { userId = result.Value.UserId, token = result.Value.Token });
        }

        [HttpGet("users/password/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyPasswordResetToken(
            [FromQuery] Guid uid,
            [FromQuery] string token,
            CancellationToken cancellationToken)
        {
            var query = new VerifyPasswordResetTokenQuery(uid, token ?? string.Empty);
            var result = await _mediator.Send(query, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok(result.Value);
        }

        [HttpGet("users/password/otp/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyPasswordResetOtp(
            [FromQuery] Guid uid,
            [FromQuery] string otp,
            CancellationToken cancellationToken)
        {
            var query = new VerifyPasswordResetOtpQuery(uid, otp ?? string.Empty);
            var result = await _mediator.Send(query, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok(result.Value);
        }

        [HttpPost("users/password/reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
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

            return Ok(new { message = "Password has been reset successfully." });
        }

        [HttpPut("profile")]
        [Authorize("Admin", "User")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request, CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("User context is missing.");
            }

            string? avatarBase64 = null;
            if (request.AvatarFile is not null && request.AvatarFile.Length > 0)
            {
                await using var memoryStream = new MemoryStream();
                await request.AvatarFile.CopyToAsync(memoryStream, cancellationToken);
                avatarBase64 = Convert.ToBase64String(memoryStream.ToArray());
            }

            var name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            var gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            var command = new UpdateUserProfileCommand(
                userId,
                name,
                email,
                request.DateOfBirth,
                gender,
                phoneNumber,
                avatarBase64);
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

            var profile = await _mediator.Send(new GetCurrentUserProfileQuery(userId), cancellationToken);
            if (profile.IsFailure)
            {
                return HandleFailure(profile);
            }

            return Ok(profile.Value);
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
        [Authorize("Admin")]
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

            var result = await _mediator.Send(new GetCurrentUserProfileQuery(userId), cancellationToken);
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

    public sealed class UpdateUserProfileRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }

    public sealed class ChangePasswordRequest
    {
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }

    public sealed class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
    public sealed record LoginWithFirebaseRequest(string IdToken);
}
