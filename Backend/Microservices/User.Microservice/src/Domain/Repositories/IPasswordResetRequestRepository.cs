using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using SharedLibrary.Common;

namespace Domain.Repositories;

public interface IPasswordResetRequestRepository : IRepository<PasswordResetRequest>
{
    Task InvalidateActiveRequestsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PasswordResetRequest?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<PasswordResetRequest?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
