using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(MyDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                return Array.Empty<string>();
            }

            var roleNames = await _context.UserRoles
                .AsNoTracking()
                .Where(relationship => relationship.UserId == userId)
                .Select(relationship => relationship.Role.RoleName)
                .Where(roleName => roleName != null)
                .Distinct()
                .ToListAsync(cancellationToken);

            return roleNames.AsReadOnly();
        }
    }
}
