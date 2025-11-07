using Application.Users;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Queries;

public sealed record VerifyPasswordResetTokenQuery(Guid UserId, string Token)
    : IQuery<PasswordResetVerificationResponse>;

public sealed record PasswordResetVerificationResponse(Guid UserId, DateTime ExpiresAt);

internal sealed class VerifyPasswordResetTokenQueryHandler
    : IQueryHandler<VerifyPasswordResetTokenQuery, PasswordResetVerificationResponse>
{
    private readonly IPasswordResetRequestRepository _passwordResetRepository;

    public VerifyPasswordResetTokenQueryHandler(IPasswordResetRequestRepository passwordResetRepository)
    {
        _passwordResetRepository = passwordResetRepository;
    }

    public async Task<Result<PasswordResetVerificationResponse>> Handle(
        VerifyPasswordResetTokenQuery query,
        CancellationToken cancellationToken)
    {
        var resetRequest = await _passwordResetRepository
            .GetActiveByTokenAsync(query.Token, cancellationToken);

        if (resetRequest is null || resetRequest.UserId != query.UserId)
        {
            return Result.Failure<PasswordResetVerificationResponse>(PasswordResetErrors.InvalidOrExpired);
        }

        return Result.Success(new PasswordResetVerificationResponse(resetRequest.UserId, resetRequest.ExpiresAt));
    }
}
