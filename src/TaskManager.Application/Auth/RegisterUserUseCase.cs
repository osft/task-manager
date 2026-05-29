using TaskManager.Application.Auth.Commands;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Users;
using TaskManager.Application.Users.Models;

namespace TaskManager.Application.Auth;

public sealed class RegisterUserUseCase
{
    public const string DefaultRoleName = "ProjectManager";

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> ExecuteAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var roleId = await _roleRepository.GetRoleIdByNameAsync(DefaultRoleName, cancellationToken);
        var userId = Guid.NewGuid();
        var createdOn = DateTime.UtcNow;

        var registration = new UserRegistration
        {
            Id = userId,
            Email = normalizedEmail,
            Name = command.Name.Trim(),
            Alias = string.IsNullOrWhiteSpace(command.Alias) ? null : command.Alias.Trim(),
            PasswordHash = _passwordHasher.Hash(command.Password),
            RoleId = roleId,
            CreatedBy = userId,
            CreatedOnUtc = createdOn
        };

        return await _userRepository.CreateAsync(registration, cancellationToken);
    }
}
