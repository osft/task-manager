namespace TaskManager.Application.Users;

public interface IRoleRepository
{
    Task<int> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken = default);
}
