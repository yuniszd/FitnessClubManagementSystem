using FCMS.Application.Abstracts;
using FCMS.Application.Extensions.Exceptions;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace FCMS.Persistence.Services;

public class QrCodeService : IQrCodeService
{
    private readonly ILogger<QrCodeService> _logger;

    public QrCodeService(ILogger<QrCodeService> logger)
    {
        _logger = logger;
    }

    public byte[] GenerateQrCode(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Content", "QR content cannot be null or empty");

        try
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate QR code for content: {Content}", content);
            throw new InternalErrorException("Failed to generate QR code", ex);
        }
    }
}
