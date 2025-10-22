using System;
using System.Collections.Generic;

namespace Application.Users.Queries
{
    public sealed record GetUserResponse(
        Guid UserId,
        string Name,
        string Email,
        bool IsVerified,
        bool IsActive,
        string? AvatarUrl,
        bool? IsPremium,
        string? ProviderName,
        DateTime? CreatedAt,
        DateTime? UpdatedAt
    );

    public sealed record GetUsersPageResponse(
        IReadOnlyList<GetUserResponse> Users,
        int PageNumber,
        int PageSize,
        int TotalCount
    );
}
