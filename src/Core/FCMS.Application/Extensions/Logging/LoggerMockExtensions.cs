using Microsoft.Extensions.Logging;
using Moq;

namespace FCMS.Application.Extensions.Logging;

public static class LoggerMockExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock,
                                    LogLevel logLevel,
                                    Times times,
                                    string? messageContains = null)
    {
        loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    messageContains == null || v.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            times
        );
    }
}
