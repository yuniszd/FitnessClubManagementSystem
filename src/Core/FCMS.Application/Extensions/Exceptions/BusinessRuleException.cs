using FCMS.Application.Extensions.Exceptions;

namespace FCMS.Application.Extensions.Exceptions
{
    public class BusinessRuleException : BaseException
    {
        public BusinessRuleException(
            string message = "Business rule violated",
            string? userMessage = "This action cannot be performed due to business rules",
            string? details = null)
            : base(
                message: message,
                errorCode: ErrorCodes.BusinessRuleViolation,
                userMessage: userMessage,
                details: details
            )
        {
        }

        public BusinessRuleException(string message, Exception innerException)
            : base(
                message: message,
                errorCode: ErrorCodes.BusinessRuleViolation,
                userMessage: "This action cannot be performed due to business rules",
                innerException: innerException
            )
        {
        }
    }
}
