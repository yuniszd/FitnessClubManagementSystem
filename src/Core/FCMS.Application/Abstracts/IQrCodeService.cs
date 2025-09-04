namespace FCMS.Application.Abstracts;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string content); // QR kodu PNG byte[] kimi qaytarır
}
