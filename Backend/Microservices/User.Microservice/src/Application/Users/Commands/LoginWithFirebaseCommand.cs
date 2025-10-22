using MediatR;
using SharedLibrary.Authentication;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Commands;

public sealed record LoginWithFirebaseCommand(string IdToken) : IRequest<Result<LoginResponse>>;
