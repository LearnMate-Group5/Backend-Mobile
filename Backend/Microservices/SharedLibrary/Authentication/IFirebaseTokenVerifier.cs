using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLibrary.Authentication
{
    public record FirebaseUserPayload(
        string FirebaseUserId,
        string Email,
        bool EmailVerified,
        string? DisplayName,
        string? PictureUrl
    );

    public interface IFirebaseTokenVerifier
    {
        Task<FirebaseUserPayload> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
    }

    public class FirebaseTokenVerificationException : Exception
    {
        public FirebaseTokenVerificationException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
