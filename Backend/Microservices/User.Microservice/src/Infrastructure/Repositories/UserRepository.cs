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
    public class UserRepository :  Repository<User>, IUserRepository
    {
        public UserRepository(MyDbContext context) : base(context)
        {
        }

        public async Task EnsureAvatarColumnSupportsLargeContentAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "ALTER TABLE IF EXISTS users ALTER COLUMN avatar_url TYPE text;";
            await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
    }
}
