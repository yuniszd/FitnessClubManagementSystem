// 📂 FCMS.Application/Abstracts/IAuthService.cs

namespace FCMS.Application.Abstracts;

public interface IAuthService
{
    /// <summary>
    /// Üzvü kart nömrəsinə əsasən login edir və cookie yaradır.
    /// </summary>
    Task LoginWithCardAsync(string cardNumber);

    /// <summary>
    /// Aktiv istifadəçini sistemdən çıxarır (cookie silinir).
    /// </summary>
    Task LogoutAsync();
}
