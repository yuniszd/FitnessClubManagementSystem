namespace FCMS.Application.Abstracts;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string content);
}
