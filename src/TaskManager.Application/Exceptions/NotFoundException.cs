namespace TaskManager.Application.Exceptions;

public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string message = "Resource not found.")
        : base(message)
    {
    }
}
