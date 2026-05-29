namespace TaskManager.Domain.Validation;

public sealed class ValidationResult
{
    private ValidationResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static ValidationResult Success() => new(true, null);

    public static ValidationResult Failure(string errorMessage) =>
        new(false, errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));
}
