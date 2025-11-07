using System;
using SharedLibrary.Abstractions.Messaging;

namespace Application.Users.Queries;

public sealed record GetCurrentUserProfileQuery(Guid UserId) : IQuery<GetCurrentUserProfileResponse>;
