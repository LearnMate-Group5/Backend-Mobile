using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Queries
{
    public sealed record GetUserByIdQuery(Guid UserId) : IQuery<GetUserResponse>;

    internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<GetUserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var userEntity = await _userRepository
                .GetAll()
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if (userEntity is null)
            {
                return Result.Failure<GetUserResponse>(new Error("User.NotFound", $"User '{request.UserId}' was not found."));
            }

            var roles = userEntity.UserRoles
                .Where(ur => ur.Role != null && !string.IsNullOrWhiteSpace(ur.Role.RoleName))
                .Select(ur => ur.Role.RoleName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();

            var user = new GetUserResponse(
                userEntity.UserId,
                userEntity.Name,
                userEntity.Email,
                userEntity.IsVerified,
                userEntity.IsActive,
                userEntity.AvatarUrl,
                userEntity.IsPremium,
                userEntity.ProviderName,
                userEntity.ProviderUserId,
                roles,
                userEntity.CreatedAt,
                userEntity.UpdatedAt);

            return Result.Success(user);
        }
    }
}
