using FCMS.Application.Abstracts;
using QRCoder;

namespace FCMS.Persistence.Services;

public class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrCode(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        // Cross-platform PNG generator
        var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(20); // 20 = pixel ölçüsü
    }
}
