namespace TaskManager.Application.Exceptions;

public sealed class UnauthorizedApplicationException : ApplicationException
{
    public UnauthorizedApplicationException(string message = "Invalid credentials.")
        : base(message)
    {
    }
}
