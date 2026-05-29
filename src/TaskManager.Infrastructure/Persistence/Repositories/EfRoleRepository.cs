using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Users;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class EfRoleRepository : IRoleRepository
{
    private readonly TaskManagerDbContext _context;
    private readonly ILogger<EfRoleRepository> _logger;

    public EfRoleRepository(TaskManagerDbContext context, ILogger<EfRoleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var roleId = await _context.Roles
                .AsNoTracking()
                .Where(r => r.Name == roleName)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (roleId == 0)
            {
                throw new InvalidOperationException($"Role '{roleName}' was not found.");
            }

            return roleId;
        }
        catch (DbUpdateException ex)
        {
            throw PersistenceExceptionHandler.Wrap(ex, _logger, nameof(GetRoleIdByNameAsync));
        }
    }
}
