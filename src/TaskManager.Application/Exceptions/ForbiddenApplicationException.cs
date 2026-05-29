namespace TaskManager.Application.Exceptions;

public sealed class ForbiddenApplicationException : ApplicationException
{
    public ForbiddenApplicationException(string message)
        : base(message)
    {
    }
}
