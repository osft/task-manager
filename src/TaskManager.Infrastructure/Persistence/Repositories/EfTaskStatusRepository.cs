using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Models;
using TaskManager.Infrastructure.Persistence.Mappers;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class EfTaskStatusRepository : ITaskStatusRepository
{
    private readonly TaskManagerDbContext _context;
    private readonly ILogger<EfTaskStatusRepository> _logger;

    public EfTaskStatusRepository(TaskManagerDbContext context, ILogger<EfTaskStatusRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskStatusRecord?> GetByIdAsync(int statusId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TaskStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == statusId, cancellationToken);

        return entity is null ? null : PersistenceMappers.ToTaskStatusRecord(entity);
    }

    public async Task<IReadOnlyList<TaskStatusRecord>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.TaskStatuses
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);

        return entities.Select(PersistenceMappers.ToTaskStatusRecord).ToList();
    }
}
