using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Extensions;

namespace Application.Users.Commands
{
    public sealed record UpdateUserProfileCommand(
        Guid UserId,
        string? Name,
        string? Email,
        DateTime? DateOfBirth,
        string? Gender,
        string? PhoneNumber,
        string? AvatarBase64
    ) : ICommand;

    internal sealed class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

                if (!string.IsNullOrWhiteSpace(command.Email))
                {
                    var normalizedEmail = command.Email.Trim().ToLowerInvariant();
                    if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        var existingUsers = await _userRepository.FindAsync(u => u.Email == normalizedEmail, cancellationToken)
                            ?? Enumerable.Empty<Domain.Entities.User>();

                        if (existingUsers.Any(u => u.UserId != command.UserId))
                        {
                            return Result.Failure(new Error("User.EmailAlreadyExists", "Email is already registered by another account."));
                        }

                        user.Email = normalizedEmail;
                    }
                }

                if (!string.IsNullOrWhiteSpace(command.Name))
                {
                    user.Name = command.Name.Trim();
                }

                if (command.DateOfBirth.HasValue)
                {
                    user.DateOfBirth = command.DateOfBirth;
                }

                if (command.Gender is not null)
                {
                    user.Gender = string.IsNullOrWhiteSpace(command.Gender) ? null : command.Gender.Trim();
                }

                if (command.PhoneNumber is not null)
                {
                    user.PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : command.PhoneNumber.Trim();
                }

                if (command.AvatarBase64 is not null)
                {
                    var trimmed = string.IsNullOrWhiteSpace(command.AvatarBase64)
                        ? null
                        : command.AvatarBase64.Trim();

                    if (trimmed is not null && trimmed.Length > 255)
                    {
                        await _userRepository.EnsureAvatarColumnSupportsLargeContentAsync(cancellationToken);
                    }

                    user.AvatarUrl = trimmed;
                }

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
}
