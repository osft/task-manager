using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using TaskManager.Application.Exceptions;
using AppException = TaskManager.Application.Exceptions.ApplicationException;

namespace TaskManager.Infrastructure.Persistence;

internal static class PersistenceExceptionHandler
{
    public static Exception Wrap(
        DbUpdateException exception,
        ILogger logger,
        string operation,
        string? uniqueViolationMessage = null)
    {
        if (IsUniqueViolation(exception))
        {
            logger.LogWarning(exception, "Unique constraint violation during {Operation}", operation);
            return new ConflictException(
                uniqueViolationMessage ?? "A conflict occurred with an existing record.");
        }

        logger.LogError(exception, "Database operation failed: {Operation}", operation);
        return new AppException("A database error occurred while processing your request.");
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
