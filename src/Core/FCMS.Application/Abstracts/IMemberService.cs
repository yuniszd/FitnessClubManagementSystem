using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Abstracts;

public interface IMemberService
{
    Task<Member> AddMemberAsync(CreateMemberDto dto);  
    Task UpdateMemberAsync(UpdateMemberDto dto);        
    Task DeleteMemberAsync(Guid id);              
    Task<Member?> GetByIdAsync(Guid id);              
    Task<IEnumerable<Member>> GetAllAsync();         
    Task<Member?> GetByCardAsync(string cardNumber);  
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