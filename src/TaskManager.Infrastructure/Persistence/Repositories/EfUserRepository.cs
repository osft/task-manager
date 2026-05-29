using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Users;
using TaskManager.Application.Users.Models;
using TaskManager.Infrastructure.Persistence.Entities;
using TaskManager.Infrastructure.Persistence.Mappers;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class EfUserRepository : IUserRepository
{
    private readonly TaskManagerDbContext _context;
    private readonly ILogger<EfUserRepository> _logger;

    public EfUserRepository(TaskManagerDbContext context, ILogger<EfUserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task<Guid> CreateAsync(UserRegistration registration, CancellationToken cancellationToken = default)
    {
        var entity = new UserEntity
        {
            Id = registration.Id,
            Email = registration.Email,
            Name = registration.Name,
            Alias = registration.Alias,
            PasswordHash = registration.PasswordHash,
            RoleId = registration.RoleId,
            IsActive = true,
            EndDate = null,
            LastLoginDate = null,
            MfaEnabled = false,
            CreatedBy = registration.CreatedBy,
            CreatedOn = registration.CreatedOnUtc,
            UpdatedBy = registration.CreatedBy,
            UpdatedOn = registration.CreatedOnUtc
        };

        _context.Users.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw PersistenceExceptionHandler.Wrap(
                ex,
                _logger,
                nameof(CreateAsync),
                "A user with this email already exists.");
        }

        return registration.Id;
    }

    public async Task<UserRecord?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var entity = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        return entity is null ? null : PersistenceMappers.ToUserRecord(entity);
    }

    public async Task<UserRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return entity is null ? null : PersistenceMappers.ToUserRecord(entity);
    }

    public async Task UpdateLastLoginAsync(
        Guid userId,
        DateTime lastLoginUtc,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.LastLoginDate = lastLoginUtc;
        entity.UpdatedBy = userId;
        entity.UpdatedOn = lastLoginUtc;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw PersistenceExceptionHandler.Wrap(ex, _logger, nameof(UpdateLastLoginAsync));
        }
    }
}
