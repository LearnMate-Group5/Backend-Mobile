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
        string Name,
        string Email,
        string? AvatarUrl
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
            var normalizedEmail = command.Email.Trim().ToLowerInvariant();
            var trimmedName = command.Name.Trim();
            var avatarUrl = string.IsNullOrWhiteSpace(command.AvatarUrl) ? null : command.AvatarUrl.Trim();

            try
            {
                var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

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

                user.Name = trimmedName;
                user.AvatarUrl = avatarUrl;
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
