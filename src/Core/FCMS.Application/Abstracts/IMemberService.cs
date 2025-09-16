using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Abstracts;

public interface IMemberService
{
    Task<Member> AddMemberAsync(CreateMemberDto dto);   // Yeni üzv əlavə et
    Task UpdateMemberAsync(Member member);             // Üzv məlumatlarını yenilə
    Task DeleteMemberAsync(Guid id);                   // Üzvü sil
    Task<Member?> GetByIdAsync(Guid id);              // Id üzrə üzv tap
    Task<IEnumerable<Member>> GetAllAsync();          // Bütün üzvləri gətir
    Task<Member?> GetByCardAsync(string cardNumber);  // CardNumber üzrə üzv tap
    Task<bool> ValidateQrAsync(string qrCode);

}