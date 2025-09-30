using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Abstracts;

public interface IMemberService
{
    Task<Member> AddMemberAsync(CreateMemberDto dto);   // Yeni üzv əlavə et
    Task UpdateMemberAsync(UpdateMemberDto dto);        // Üzv məlumatlarını yenilə
    Task DeleteMemberAsync(Guid id);                   // Üzvü sil
    Task<Member?> GetByIdAsync(Guid id);              // Id üzrə üzv tap
    Task<IEnumerable<Member>> GetAllAsync();          // Bütün üzvləri gətir
    Task<Member?> GetByCardAsync(string cardNumber);  // CardNumber üzrə üzv tap
    Task<bool> ValidateQrAsync(string qrCode);

    Task<(IEnumerable<Member> Members, int TotalCount)> GetPagedAsync(
    int pageNumber,
    int pageSize);

    Task<(IEnumerable<Member> Members, int TotalCount)> SearchPagedAsync(
        string? fullName,
        string? cardNumber,
        bool? isActive,
        int pageNumber,
        int pageSize);

}