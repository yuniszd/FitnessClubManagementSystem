namespace FCMS.Application.Extensions.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(
        string message = "Forbidden access",
        string? userMessage = "You do not have permission to perform this action")
        : base(
            message: message,
            errorCode: ErrorCodes.Forbidden,
            userMessage: userMessage
        )
    {
    }

    public ForbiddenException(string message, Exception innerException)
        : base(
            message: message,
            errorCode: ErrorCodes.Forbidden,
            userMessage: "You do not have permission to perform this action",
            innerException: innerException
        )
    {
    }
}
