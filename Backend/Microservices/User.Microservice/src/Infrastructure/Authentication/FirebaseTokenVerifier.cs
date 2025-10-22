using System;
using FirebaseAdmin.Auth;
using SharedLibrary.Authentication;

namespace Infrastructure.Authentication
{
    public class FirebaseTokenVerifier : IFirebaseTokenVerifier
    {
        public async Task<FirebaseUserPayload> VerifyAsync(string idToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new FirebaseTokenVerificationException("Firebase ID token must be provided.");
            }

            try
            {
                var payload = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken, cancellationToken);

                var email = payload.Claims.TryGetValue("email", out var emailObj)
                    ? emailObj?.ToString()
                    : null;
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new FirebaseTokenVerificationException("Firebase token does not contain an email address.");
                }

                string? displayName = null;
                if (payload.Claims.TryGetValue("name", out var nameValue))
                {
                    displayName = nameValue?.ToString();
                }
                else if (payload.Claims.TryGetValue("displayName", out var displayNameValue))
                {
                    displayName = displayNameValue?.ToString();
                }

                string? pictureUrl = null;
                if (payload.Claims.TryGetValue("picture", out var pictureValue))
                {
                    pictureUrl = pictureValue?.ToString();
                }
                else if (payload.Claims.TryGetValue("photoUrl", out var photoUrlValue))
                {
                    pictureUrl = photoUrlValue?.ToString();
                }

                var emailVerified = false;
                if (payload.Claims.TryGetValue("email_verified", out var emailVerifiedObj) &&
                    bool.TryParse(emailVerifiedObj?.ToString(), out var parsedVerified))
                {
                    emailVerified = parsedVerified;
                }

                return new FirebaseUserPayload(
                    FirebaseUserId: payload.Uid,
                    Email: email,
                    EmailVerified: emailVerified,
                    DisplayName: displayName,
                    PictureUrl: pictureUrl
                );
            }
            catch (FirebaseAuthException ex)
            {
                throw new FirebaseTokenVerificationException("Firebase token is invalid.", ex);
            }
            catch (Exception ex)
            {
                throw new FirebaseTokenVerificationException("Failed to verify Firebase token.", ex);
            }
        }
    }
}
