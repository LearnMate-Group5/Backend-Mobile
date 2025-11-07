using Application.Users;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Authentication;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Queries;

public sealed record VerifyPasswordResetOtpQuery(Guid UserId, string Otp)
    : IQuery<PasswordResetVerificationResponse>;

internal sealed class VerifyPasswordResetOtpQueryHandler
    : IQueryHandler<VerifyPasswordResetOtpQuery, PasswordResetVerificationResponse>
{
    private readonly IPasswordResetRequestRepository _passwordResetRepository;
    private readonly IPasswordHasher _passwordHasher;

    public VerifyPasswordResetOtpQueryHandler(
        IPasswordResetRequestRepository passwordResetRepository,
        IPasswordHasher passwordHasher)
    {
        _passwordResetRepository = passwordResetRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<PasswordResetVerificationResponse>> Handle(
        VerifyPasswordResetOtpQuery query,
        CancellationToken cancellationToken)
    {
        var resetRequest = await _passwordResetRepository
            .GetActiveByUserIdAsync(query.UserId, cancellationToken);

        if (resetRequest is null)
        {
            return Result.Failure<PasswordResetVerificationResponse>(PasswordResetErrors.InvalidOrExpired);
        }

        var otp = query.Otp?.Trim() ?? string.Empty;
        if (!_passwordHasher.VerifyPassword(otp, resetRequest.OtpHash))
        {
            return Result.Failure<PasswordResetVerificationResponse>(PasswordResetErrors.InvalidOrExpired);
        }

        return Result.Success(new PasswordResetVerificationResponse(resetRequest.UserId, resetRequest.ExpiresAt));
    }
}
