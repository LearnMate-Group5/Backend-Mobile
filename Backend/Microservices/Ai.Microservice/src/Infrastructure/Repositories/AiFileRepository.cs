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
