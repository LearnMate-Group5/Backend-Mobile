using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using SharedLibrary.Common;

namespace Domain.Repositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
