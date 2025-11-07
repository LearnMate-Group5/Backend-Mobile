using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Authentication;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Extensions;

namespace Application.Users.Commands;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string OldPassword,
    string NewPassword,
    string ConfirmNewPassword
) : ICommand;

internal sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var oldPassword = command.OldPassword?.Trim() ?? string.Empty;
        var newPassword = command.NewPassword?.Trim() ?? string.Empty;
        var confirmPassword = command.ConfirmNewPassword?.Trim() ?? string.Empty;

        try
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return Result.Failure(new Error("User.PasswordNotSet", "Password authentication is not configured for this account."));
            }

            if (!_passwordHasher.VerifyPassword(oldPassword, user.PasswordHash))
            {
                return Result.Failure(new Error("User.InvalidOldPassword", "The current password provided is incorrect."));
            }

            if (string.Equals(oldPassword, newPassword, StringComparison.Ordinal))
            {
                return Result.Failure(new Error("User.PasswordUnchanged", "New password must be different from the current password."));
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                return Result.Failure(new Error("User.PasswordMismatch", "Password confirmation does not match the new password."));
            }

            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = DateTimeExtensions.PostgreSqlUtcNow;

            _userRepository.Update(user);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error("User.NotFound", $"User '{command.UserId}' was not found."));
        }

        return Result.Success();
    }
}
