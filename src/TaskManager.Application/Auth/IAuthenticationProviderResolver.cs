namespace TaskManager.Application.Auth;

public interface IAuthenticationProviderResolver
{
    IAuthenticationProvider Resolve(string provider);
}
