using FCMS.Application.Extensions.Exceptions;

public class ValidationException : BaseException
{
    public string[] Errors { get; }
    public Dictionary<string, string[]> FieldErrors { get; }

    // Əsas constructor
    public ValidationException(
        string[] errors,
        Dictionary<string, string[]>? fieldErrors = null,
        string? message = null,
        string? userMessage = null)
        : base(
            message: message ?? "Validation failed",
            errorCode: ErrorCodes.ValidationError,
            userMessage: userMessage ?? "Please correct the validation errors and try again",
            details: errors != null ? string.Join("; ", errors) : null
        )
    {
        Errors = errors ?? Array.Empty<string>();
        FieldErrors = fieldErrors ?? new Dictionary<string, string[]>();
    }

    // Field errors üçün convenience constructor
    public ValidationException(Dictionary<string, string[]> fieldErrors)
        : this(
            errors: fieldErrors.SelectMany(x => x.Value).ToArray(),
            fieldErrors: fieldErrors,
            message: "One or more field validation errors occurred"
        )
    {
    }

    // Single error üçün convenience constructor
    public ValidationException(string field, string error)
        : this(
            errors: new[] { error },
            fieldErrors: new Dictionary<string, string[]> { { field, new[] { error } } },
            message: $"Validation error for field '{field}': {error}"
        )
    {
    }
}