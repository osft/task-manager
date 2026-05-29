using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Models;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence.Mappers;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class EfTaskRepository : ITaskRepository
{
    private readonly TaskManagerDbContext _context;
    private readonly ILogger<EfTaskRepository> _logger;

    public EfTaskRepository(TaskManagerDbContext context, ILogger<EfTaskRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskRecord?> GetByIdForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tasks
            .AsNoTracking()
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId, cancellationToken);

        return entity is null ? null : PersistenceMappers.ToTaskRecord(entity);
    }

    public async Task<IReadOnlyList<TaskRecord>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Tasks
            .AsNoTracking()
            .Include(t => t.Status)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync(cancellationToken);

        return entities.Select(PersistenceMappers.ToTaskRecord).ToList();
    }

    public async Task<TaskRecord> CreateAsync(
        TaskItem task,
        Guid auditUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = PersistenceMappers.ToTaskEntity(task, auditUserId);
        _context.Tasks.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw PersistenceExceptionHandler.Wrap(ex, _logger, nameof(CreateAsync));
        }

        await _context.Entry(entity).Reference(e => e.Status).LoadAsync(cancellationToken);
        return PersistenceMappers.ToTaskRecord(entity);
    }

    public async Task<TaskRecord?> UpdateAsync(TaskRecord task, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == task.Id && t.UserId == task.UserId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        PersistenceMappers.ApplyTaskRecord(entity, task);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw PersistenceExceptionHandler.Wrap(ex, _logger, nameof(UpdateAsync));
        }

        await _context.Entry(entity).Reference(e => e.Status).LoadAsync(cancellationToken);
        return PersistenceMappers.ToTaskRecord(entity);
    }

    public async Task<bool> DeleteAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.Tasks.Remove(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw PersistenceExceptionHandler.Wrap(ex, _logger, nameof(DeleteAsync));
        }

        return true;
    }
}
