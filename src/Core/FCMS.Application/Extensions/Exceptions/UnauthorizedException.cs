namespace FCMS.Application.Extensions.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(
            string message = "Unauthorized access",
            string? userMessage = "You are not authorized to perform this action")
            : base(
                message: message,
                errorCode: ErrorCodes.Unauthorized,
                userMessage: userMessage
            )
        {
        }

        public UnauthorizedException(string message, Exception innerException)
            : base(
                message: message,
                errorCode: ErrorCodes.Unauthorized,
                userMessage: "You are not authorized to perform this action",
                innerException: innerException
            )
        {
        }
    }
}
