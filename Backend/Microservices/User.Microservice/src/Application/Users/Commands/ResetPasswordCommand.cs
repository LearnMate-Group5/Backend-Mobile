using Application.Users;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Authentication;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Extensions;

namespace Application.Users.Commands;

public sealed record ResetPasswordCommand(
    string Email,
    string? Token,
    string? Otp,
    string NewPassword,
    string ConfirmNewPassword) : ICommand;

internal sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRequestRepository _passwordResetRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetRequestRepository passwordResetRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordResetRepository = passwordResetRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return PasswordResetErrors.InvalidOrExpired;
        }

        var token = command.Token?.Trim();
        var otp = command.Otp?.Trim();

        // Ignore token if it's a placeholder value
        if (string.Equals(token, "string", StringComparison.OrdinalIgnoreCase))
        {
            token = null;
        }

        PasswordResetRequest? resetRequest = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            resetRequest = await _passwordResetRepository.GetActiveByTokenAsync(token, cancellationToken);
            if (resetRequest is null || resetRequest.UserId != user.UserId)
            {
                return PasswordResetErrors.InvalidOrExpired;
            }
        }
        else if (!string.IsNullOrWhiteSpace(otp))
        {
            resetRequest = await _passwordResetRepository.GetActiveByUserIdAsync(user.UserId, cancellationToken);
            if (resetRequest is null || !_passwordHasher.VerifyPassword(otp, resetRequest.OtpHash))
            {
                return PasswordResetErrors.InvalidOrExpired;
            }
        }
        else
        {
            return PasswordResetErrors.MethodRequired;
        }

        user.PasswordHash = _passwordHasher.HashPassword(command.NewPassword.Trim());
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTimeExtensions.PostgreSqlUtcNow;

        resetRequest.Used = true;

        _userRepository.Update(user);
        _passwordResetRepository.Update(resetRequest);

        return Result.Success();
    }
}
