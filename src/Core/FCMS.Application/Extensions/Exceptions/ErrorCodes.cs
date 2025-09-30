namespace FCMS.Application.Extensions.Exceptions;

public static class ErrorCodes
{
    // Validation
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InvalidInput = "INVALID_INPUT";

    // Not Found
    public const string NotFound = "NOT_FOUND";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string UserNotFound = "USER_NOT_FOUND";

    // Auth
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InvalidToken = "INVALID_TOKEN";

    // Business
    public const string BadRequest = "BAD_REQUEST";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
    public const string DuplicateEntry = "DUPLICATE_ENTRY";

    // System
    public const string InternalError = "INTERNAL_ERROR";
}