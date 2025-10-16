using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Extensions;

namespace Application.Users.Commands
{
    public sealed record UpdateUserRoleCommand(Guid UserId, string RoleName) : ICommand;

    internal sealed class UpdateUserRoleCommandHandler : ICommandHandler<UpdateUserRoleCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public UpdateUserRoleCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<Result> Handle(UpdateUserRoleCommand command, CancellationToken cancellationToken)
        {
            if (!UserRoleCatalog.TryGetCanonicalName(command.RoleName, out var canonicalRoleName))
            {
                return Result.Failure(new Error("User.Role.Invalid", $"Supported roles: {string.Join(", ", UserRoleCatalog.SupportedRoles)}"));
            }

            User user;
            try
            {
                user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            }
            catch (KeyNotFoundException)
            {
                return Result.Failure(new Error("User.NotFound", $"User '{command.UserId}' was not found."));
            }

            var role = await _roleRepository.GetByNameAsync(canonicalRoleName, cancellationToken);
            if (role is null)
            {
                role = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = canonicalRoleName,
                    CreatedAt = DateTimeExtensions.PostgreSqlUtcNow
                };

                await _roleRepository.AddAsync(role, cancellationToken);
            }

            var currentAssignments = await _userRoleRepository.FindAsync(ur => ur.UserId == user.UserId, cancellationToken)
                                      ?? Enumerable.Empty<UserRole>();
            var assignmentsList = currentAssignments.ToList();

            if (assignmentsList.Count == 1 && assignmentsList[0].RoleId == role.RoleId)
            {
                return Result.Success();
            }

            if (assignmentsList.Count > 0)
            {
                _userRoleRepository.DeleteRange(assignmentsList);
                user.UserRoles.Clear();
            }

            var newAssignment = new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                AssignedAt = DateTimeExtensions.PostgreSqlUtcNow
            };

            await _userRoleRepository.AddAsync(newAssignment, cancellationToken);
            user.UserRoles.Add(newAssignment);

            return Result.Success();
        }
    }
}
