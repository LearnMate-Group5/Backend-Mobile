using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Abstractions.Messaging;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries
{
    public sealed record GetAllUsersQuery(int PageNumber = 1, int PageSize = 20) : IQuery<GetUsersPageResponse>;

    internal sealed class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, GetUsersPageResponse>
    {
        private const int MaxPageSize = 100;
        private readonly IUserRepository _userRepository;

        public GetAllUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<GetUsersPageResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, MaxPageSize);

            var query = _userRepository.GetAll();

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt ?? DateTime.UnixEpoch)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var userResponses = users
                .Select(u =>
                {
                    var roles = u.UserRoles
                        .Where(ur => ur.Role != null && !string.IsNullOrWhiteSpace(ur.Role.RoleName))
                        .Select(ur => ur.Role.RoleName.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList()
                        .AsReadOnly();

                    return new GetUserResponse(
                        u.UserId,
                        u.Name,
                        u.Email,
                        u.IsVerified,
                        u.IsActive,
                        u.AvatarUrl,
                        u.IsPremium,
                        u.ProviderName,
                        u.ProviderUserId,
                        roles,
                        u.CreatedAt,
                        u.UpdatedAt);
                })
                .ToList();

            var result = new GetUsersPageResponse(userResponses, pageNumber, pageSize, totalCount);

            return Result.Success(result);
        }
    }
}
