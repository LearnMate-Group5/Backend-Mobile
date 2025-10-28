using System;
using System.Collections.Generic;
using SharedLibrary.Abstractions.Messaging;

namespace Application.Users.Queries
{
    public sealed record GetUserRolesQuery(Guid UserId) : IQuery<IReadOnlyList<string>>;
}
