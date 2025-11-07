using SharedLibrary.Common.ResponseModel;

namespace Application.Users;

internal static class PasswordResetErrors
{
    public static readonly Error InvalidOrExpired =
        new("PasswordReset.InvalidOrExpired", "The password reset request is invalid or has expired.");

    public static readonly Error MethodRequired =
        new("PasswordReset.MethodRequired", "You must provide either a reset token or an OTP.");
}
