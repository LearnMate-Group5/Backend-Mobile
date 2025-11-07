using System;
using System.Collections.Generic;

namespace Application.Users.Queries;

public sealed record GetCurrentUserProfileResponse(
    Guid UserId,
    string Name,
    string Email,
    DateTime? DateOfBirth,
    string? Gender,
    string? PhoneNumber,
    string Status,
    bool IsVerified,
    bool IsActive,
    string? AvatarUrl,
    bool? IsPremium,
    string? ProviderName,
    DateTime? CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<string> Roles
);
