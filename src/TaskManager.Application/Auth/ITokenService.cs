using TaskManager.Application.Auth.Models;

namespace TaskManager.Application.Auth;

public interface ITokenService
{
    AuthTokenResult CreateToken(AuthenticatedUser user);
}
