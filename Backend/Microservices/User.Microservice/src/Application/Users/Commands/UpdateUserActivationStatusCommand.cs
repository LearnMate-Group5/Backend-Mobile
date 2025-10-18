using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Commands
{
    public sealed record UpdateUserActivationStatusCommand(Guid UserId, bool IsActive) : ICommand;

    internal sealed class UpdateUserActivationStatusCommandHandler : ICommandHandler<UpdateUserActivationStatusCommand>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserActivationStatusCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> Handle(UpdateUserActivationStatusCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

                if (user.IsActive == command.IsActive)
                {
                    return Result.Success();
                }

                user.IsActive = command.IsActive;
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
