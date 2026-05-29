using FluentAssertions;
using Moq;
using TaskManager.Application.Auth;
using TaskManager.Application.Auth.Commands;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Users;
using TaskManager.Application.Users.Models;

namespace TaskManager.Application.Tests.Auth;

public class RegisterUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly RegisterUserUseCase _sut;

    public RegisterUserUseCaseTests()
    {
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");
        _roleRepository
            .Setup(r => r.GetRoleIdByNameAsync(RegisterUserUseCase.DefaultRoleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _sut = new RegisterUserUseCase(
            _userRepository.Object,
            _roleRepository.Object,
            _passwordHasher.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailIsNew_CreatesUserWithProjectManagerRole()
    {
        _userRepository.Setup(r => r.ExistsByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository.Setup(r => r.CreateAsync(It.IsAny<UserRegistration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var userId = await _sut.ExecuteAsync(new RegisterUserCommand
        {
            Name = "Test User",
            Alias = "tester",
            Email = "User@Test.com",
            Password = "Password123!"
        });

        userId.Should().NotBe(Guid.Empty);
        _userRepository.Verify(
            r => r.CreateAsync(
                It.Is<UserRegistration>(u =>
                    u.Email == "user@test.com" &&
                    u.Name == "Test User" &&
                    u.Alias == "tester" &&
                    u.RoleId == 1 &&
                    u.CreatedBy == u.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailExists_ThrowsConflictException()
    {
        _userRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.ExecuteAsync(new RegisterUserCommand
        {
            Name = "Test",
            Email = "exists@test.com",
            Password = "Password123!"
        });

        await act.Should().ThrowAsync<ConflictException>();
    }
}
