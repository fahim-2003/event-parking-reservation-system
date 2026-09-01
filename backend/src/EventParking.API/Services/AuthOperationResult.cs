namespace EventParking.API.Services;

public sealed class AuthOperationResult<T>
{
    public bool Succeeded { get; init; }

    public T? Value { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static AuthOperationResult<T> Success(T value)
    {
        return new AuthOperationResult<T>
        {
            Succeeded = true,
            Value = value
        };
    }

    public static AuthOperationResult<T> Failure(
        string errorCode,
        string errorMessage,
        IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        return new AuthOperationResult<T>
        {
            Succeeded = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            ValidationErrors = validationErrors
        };
    }
}