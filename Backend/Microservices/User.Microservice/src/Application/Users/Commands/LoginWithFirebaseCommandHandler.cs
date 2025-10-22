using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using Domain.Repositories;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Abstractions.UnitOfWork;
using SharedLibrary.Authentication;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.UserCreating;
using SharedLibrary.Extensions;

namespace Application.Users.Commands;

public class LoginWithFirebaseCommandHandler : IRequestHandler<LoginWithFirebaseCommand, Result<LoginResponse>>
{
    private const string FirebaseProviderName = "Firebase";

    private readonly IFirebaseTokenVerifier _tokenVerifier;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public LoginWithFirebaseCommandHandler(
        IFirebaseTokenVerifier tokenVerifier,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _tokenVerifier = tokenVerifier;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<LoginResponse>> Handle(LoginWithFirebaseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return Result.Failure<LoginResponse>(new Error("Auth.InvalidToken", "Firebase ID token is required."));
        }

        FirebaseUserPayload payload;
        try
        {
            payload = await _tokenVerifier.VerifyAsync(request.IdToken, cancellationToken);
        }
        catch (FirebaseTokenVerificationException ex)
        {
            return Result.Failure<LoginResponse>(new Error("Auth.InvalidToken", ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginResponse>(new Error("Auth.FirebaseError", $"Unable to verify Firebase token: {ex.Message}"));
        }

        var normalizedEmail = payload.Email.Trim().ToLowerInvariant();
        var userQuery = _userRepository
            .GetAll()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role);

        var user = await userQuery.FirstOrDefaultAsync(
            u => u.ProviderName == FirebaseProviderName && u.ProviderUserId == payload.FirebaseUserId,
            cancellationToken);

        if (user == null)
        {
            user = await userQuery.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        }

        if (user == null)
        {
            user = new User
            {
                UserId = Guid.NewGuid(),
                Email = normalizedEmail,
                Name = string.IsNullOrWhiteSpace(payload.DisplayName) ? normalizedEmail : payload.DisplayName.Trim(),
                IsVerified = payload.EmailVerified,
                IsActive = true,
                CreatedAt = DateTimeExtensions.PostgreSqlUtcNow,
                ProviderName = FirebaseProviderName,
                ProviderUserId = payload.FirebaseUserId,
                AvatarUrl = payload.PictureUrl,
                PasswordHash = null
            };

            await _userRepository.AddAsync(user, cancellationToken);

            var userRole = await _roleRepository.GetByNameAsync("User", cancellationToken);
            if (userRole == null)
            {
                userRole = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "User",
                    CreatedAt = DateTimeExtensions.PostgreSqlUtcNow
                };

                await _roleRepository.AddAsync(userRole, cancellationToken);
            }

            await _userRoleRepository.AddAsync(new UserRole
            {
                UserId = user.UserId,
                RoleId = userRole.RoleId,
                AssignedAt = DateTimeExtensions.PostgreSqlUtcNow
            }, cancellationToken);

            await _publishEndpoint.Publish(new UserCreatingSagaStart
            {
                CorrelationId = Guid.NewGuid(),
                Name = user.Name,
                Email = user.Email
            }, cancellationToken);
        }
        else
        {
            user.ProviderName = FirebaseProviderName;
            user.ProviderUserId = payload.FirebaseUserId;

            if (!string.IsNullOrWhiteSpace(payload.DisplayName))
            {
                user.Name = payload.DisplayName.Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(payload.PictureUrl))
        {
            user.AvatarUrl = payload.PictureUrl;
        }

        user.IsVerified = payload.EmailVerified;
        user.UpdatedAt = DateTimeExtensions.PostgreSqlUtcNow;

        var roles = user.UserRoles?.Select(ur => ur.Role.RoleName).ToList() ?? new List<string>();
        if (!roles.Any())
        {
            roles.Add("User");
        }

        var accessToken = _jwtTokenService.GenerateToken(user.UserId, user.Email, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTimeExtensions.PostgreSqlUtcNow.AddDays(7);

        var response = new LoginResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTimeExtensions.PostgreSqlUtcNow.AddMinutes(60),
            User: new UserInfo(user.UserId, user.Name, user.Email, roles)
        );

        return Result.Success(response);
    }
}
