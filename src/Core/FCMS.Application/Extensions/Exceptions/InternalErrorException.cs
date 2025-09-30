namespace FCMS.Application.Extensions.Exceptions;

public class InternalErrorException : BaseException
{
    public InternalErrorException(
        string message = "Internal server error",
        string? userMessage = "An unexpected error occurred",
        string? details = null)
        : base(
            message: message,
            errorCode: ErrorCodes.InternalError,
            userMessage: userMessage,
            details: details
        )
    {
    }

    public InternalErrorException(string message, Exception innerException)
        : base(
            message: message,
            errorCode: ErrorCodes.InternalError,
            userMessage: "An unexpected error occurred",
            innerException: innerException
        )
    {
    }
}
