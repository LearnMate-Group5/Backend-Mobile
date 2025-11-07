using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Constants;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Queries;

internal sealed class GetCurrentUserProfileQueryHandler : IQueryHandler<GetCurrentUserProfileQuery, GetCurrentUserProfileResponse>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<GetCurrentUserProfileResponse>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAll()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<GetCurrentUserProfileResponse>(new Error("User.NotFound", "User not found."));
        }

        var roles = user.UserRoles?
            .Select(ur => ur.Role?.RoleName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? new List<string>();

        var response = new GetCurrentUserProfileResponse(
            user.UserId,
            user.Name,
            user.Email,
            user.DateOfBirth,
            user.Gender,
            user.PhoneNumber,
            user.IsActive ? UserStatus.Active : UserStatus.Inactive,
            user.IsVerified,
            user.IsActive,
            user.AvatarUrl,
            user.IsPremium,
            user.ProviderName,
            user.CreatedAt,
            user.UpdatedAt,
            roles.AsReadOnly()
        );

        return Result.Success(response);
    }
}
