using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; } = true;

    public string? AvatarUrl { get; set; }

    public bool? IsPremium { get; set; }

    public string? Gender { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ProviderName { get; set; }

    public string? ProviderUserId { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; } = new List<PasswordResetRequest>();
}
