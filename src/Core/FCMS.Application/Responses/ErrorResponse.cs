namespace FCMS.Application.DTOs;

public class ErrorResponse
{
    public string ErrorCode { get; set; } = "INTERNAL_ERROR"; // Default
    public string Message { get; set; } = "An unexpected error occurred";
    public string? UserMessage { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public IDictionary<string, string[]>? FieldErrors { get; set; } // Validation üçün

    public ErrorResponse() { }

    public ErrorResponse(string errorCode, string message, string? userMessage = null, string? details = null, IDictionary<string, string[]>? fieldErrors = null)
    {
        ErrorCode = errorCode;
        Message = message;
        UserMessage = userMessage;
        Details = details;
        FieldErrors = fieldErrors;
    }
}
