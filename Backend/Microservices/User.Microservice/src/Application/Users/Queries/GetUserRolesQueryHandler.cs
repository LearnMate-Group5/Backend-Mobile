using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Queries
{
    internal sealed class GetUserRolesQueryHandler : IQueryHandler<GetUserRolesQuery, IReadOnlyList<string>>
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public GetUserRolesQueryHandler(IUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        public async Task<Result<IReadOnlyList<string>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            var roleNames = await _userRoleRepository.GetRoleNamesByUserIdAsync(request.UserId, cancellationToken);

            var normalized = roleNames
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();

            return Result.Success<IReadOnlyList<string>>(normalized);
        }
    }
}
