namespace FCMS.Application.Extensions.Exceptions;

public class NotFoundException : BaseException
{
    // Resource-based constructor (ƏN ÇOX İSTİFADƏ OLUNACAQ)
    public NotFoundException(string resourceType, object resourceId)
        : base(
            message: $"{resourceType} with id '{resourceId}' was not found",
            errorCode: ErrorCodes.ResourceNotFound,
            userMessage: $"The requested {resourceType.ToLower()} was not found",
            details: $"Resource: {resourceType}, ID: {resourceId}"
        )
    {
    }

    // General constructor
    public NotFoundException(string message, string? userMessage = null)
        : base(
            message: message,
            errorCode: ErrorCodes.NotFound,
            userMessage: userMessage ?? "The requested resource was not found"
        )
    {
    }

    // Inner exception ilə
    public NotFoundException(string message, Exception innerException)
        : base(
            message: message,
            errorCode: ErrorCodes.NotFound,
            userMessage: "The requested resource was not found",
            innerException: innerException
        )
    {
    }
}
