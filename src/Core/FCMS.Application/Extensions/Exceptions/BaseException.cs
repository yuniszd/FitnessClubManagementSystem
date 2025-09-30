using System;
using System.Runtime.Serialization;

namespace FCMS.Application.Extensions.Exceptions
{
    [Serializable]
    public abstract class BaseException : Exception
    {
        /// <summary>
        /// Unique error code for API consumers and logging
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// UTC timestamp when exception was created
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// User-friendly message for displaying in UI or API response
        /// </summary>
        public string UserMessage { get; }

        /// <summary>
        /// Optional detailed message for developers / logs
        /// </summary>
        public string? Details { get; }

        /// <summary>
        /// Main constructor
        /// </summary>
        protected BaseException(
            string message,
            string errorCode,
            string? userMessage = null,
            string? details = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Timestamp = DateTime.UtcNow;
            UserMessage = userMessage ?? message;
            Details = details;
        }

        /// <summary>
        /// Backward-compatible simple constructor
        /// </summary>
        protected BaseException(string message, string errorCode)
            : this(message, errorCode, null, null, null)
        {
        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        protected BaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode)) ?? "UNKNOWN_ERROR";
            Timestamp = info.GetDateTime(nameof(Timestamp));
            UserMessage = info.GetString(nameof(UserMessage)) ?? "An error occurred";
            Details = info.GetString(nameof(Details));
        }

        /// <summary>
        /// Serialization support for older formatters
        /// </summary>
        [Obsolete("This API supports obsolete formatter-based serialization.")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorCode), ErrorCode);
            info.AddValue(nameof(Timestamp), Timestamp);
            info.AddValue(nameof(UserMessage), UserMessage);
            info.AddValue(nameof(Details), Details);
        }
    }
}
