using TaskManager.Application.Exceptions;
using TaskManager.Application.Users.Models;

namespace TaskManager.Application.Users;

public sealed class GetUserProfileUseCase
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfile> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedApplicationException("User not found.");
        }

        return new UserProfile
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Alias = user.Alias,
            RoleName = user.RoleName,
            IsActive = user.IsActive,
            LastLoginDate = user.LastLoginDate
        };
    }
}
