using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AiFileRepository : IAiFileRepository
{
    private readonly MyDbContext _context;

    public AiFileRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<AiFile?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken)
    {
        return await _context.AiFiles
            .FirstOrDefaultAsync(file => file.FileId == fileId, cancellationToken);
    }

    public async Task<IReadOnlyList<AiFile>> GetAsync(bool includeAll, string userId, CancellationToken cancellationToken)
    {
        IQueryable<AiFile> query = _context.AiFiles
            .AsNoTracking()
            .Where(file => file.Status == AiFileStatus.Active);

        if (!includeAll)
        {
            var normalizedUserId = (userId ?? string.Empty).Trim().ToLowerInvariant();
            query = query.Where(file => file.UserId != null && file.UserId.ToLower() == normalizedUserId);
        }

        return await query
            .OrderByDescending(file => file.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(AiFile file, CancellationToken cancellationToken)
    {
        if (_context.Entry(file).State == EntityState.Detached)
        {
            _context.AiFiles.Attach(file);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var entity = await _context.AiFiles
            .FirstOrDefaultAsync(file => file.FileId == fileId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        _context.AiFiles.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(AiFile file, CancellationToken cancellationToken)
    {
        var existing = await _context.AiFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FileId == file.FileId, cancellationToken);

        if (existing is null)
        {
            await _context.AiFiles.AddAsync(file, cancellationToken);
        }
        else
        {
            _context.AiFiles.Update(file);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
