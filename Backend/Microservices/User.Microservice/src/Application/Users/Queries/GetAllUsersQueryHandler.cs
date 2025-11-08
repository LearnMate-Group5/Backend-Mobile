using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Abstractions.Messaging;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Domain.Constants;

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

            var query = _userRepository
                .GetAll()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);

            var totalCount = await query.CountAsync(cancellationToken);

            var usersPage = await query
                .OrderByDescending(u => u.CreatedAt ?? DateTime.UnixEpoch)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var users = usersPage
                .Select(u =>
                {
                    var role = u.UserRoles?
                        .Select(ur => ur.Role?.RoleName)
                        .FirstOrDefault(roleName => !string.IsNullOrWhiteSpace(roleName))
                        ?.Trim();

                    return new GetUserResponse(
                        u.UserId,
                        u.Name,
                        u.Email,
                        u.DateOfBirth,
                        u.Gender,
                        u.PhoneNumber,
                        u.IsActive ? UserStatus.Active : UserStatus.Inactive,
                        u.IsVerified,
                        u.IsActive,
                        u.AvatarUrl,
                        u.IsPremium,
                        u.ProviderName,
                        u.CreatedAt,
                        u.UpdatedAt,
                        role);
                })
                .ToList();

            var result = new GetUsersPageResponse(users, pageNumber, pageSize, totalCount);

            return Result.Success(result);
        }
    }
}
