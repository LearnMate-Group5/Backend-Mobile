using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Authentication;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Configs;
using SharedLibrary.Extensions;

namespace Application.Users.Commands;

public sealed record RequestPasswordResetCommand(string Email) : ICommand<PasswordResetResponse>;

public sealed record PasswordResetResponse(Guid UserId, string Token);

internal sealed class RequestPasswordResetCommandHandler : ICommandHandler<RequestPasswordResetCommand, PasswordResetResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRequestRepository _passwordResetRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly EnvironmentConfig _environmentConfig;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IPasswordResetRequestRepository passwordResetRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        EnvironmentConfig environmentConfig)
    {
        _userRepository = userRepository;
        _passwordResetRepository = passwordResetRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _environmentConfig = environmentConfig;
    }

    public async Task<Result<PasswordResetResponse>> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            // Do not disclose whether the email exists.
            return Result.Success(new PasswordResetResponse(Guid.Empty, string.Empty));
        }

        await _passwordResetRepository.InvalidateActiveRequestsAsync(user.UserId, cancellationToken);

        var otp = GenerateOtp(_environmentConfig.PasswordResetOtpLength);
        var token = Guid.NewGuid().ToString("N");

        var resetRequest = new PasswordResetRequest
        {
            PasswordResetRequestId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = token,
            OtpHash = _passwordHasher.HashPassword(otp),
            CreatedAt = DateTimeExtensions.PostgreSqlUtcNow,
            ExpiresAt = DateTimeExtensions.PostgreSqlUtcNow.AddMinutes(_environmentConfig.PasswordResetTokenExpiryMinutes),
            Used = false
        };

        await _passwordResetRepository.AddAsync(resetRequest, cancellationToken);

        var resetLink = BuildResetLink(normalizedEmail, token);
        var subject = _environmentConfig.PasswordResetEmailSubject;
        var body = BuildEmailBody(user.Name, otp, resetLink, resetRequest.ExpiresAt);

        await _emailSender.SendAsync(normalizedEmail, subject, body, cancellationToken);

        return Result.Success(new PasswordResetResponse(user.UserId, token));
    }

    private string BuildResetLink(string email, string token)
    {
        return _environmentConfig.PasswordResetLinkTemplate
            .Replace("{token}", Uri.EscapeDataString(token))
            .Replace("{email}", Uri.EscapeDataString(email));
    }

    private static string GenerateOtp(int length)
    {
        length = length <= 0 ? 6 : length;
        var digits = new char[length];
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[length];
        rng.GetBytes(buffer);

        for (var i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + (buffer[i] % 10));
        }

        return new string(digits);
    }

    private static string BuildEmailBody(string name, string otp, string resetLink, DateTime expiresAt)
    {
        var builder = new StringBuilder();
        builder.Append($"<p>Hello {System.Net.WebUtility.HtmlEncode(name)},</p>");
        builder.Append("<p>We received a request to reset your password. Use the OTP below or click the reset link:</p>");
        builder.Append($"<p><strong>OTP:</strong> {otp}</p>");
        builder.Append($"<p><a href=\"{resetLink}\">Reset your password</a></p>");
        builder.Append($"<p>This code will expire at {expiresAt:u}.</p>");
        builder.Append("<p>If you didn't initiate this request, you can ignore this email.</p>");
        builder.Append("<p>Thanks,<br/>LearnMate Support</p>");
        return builder.ToString();
    }
}
