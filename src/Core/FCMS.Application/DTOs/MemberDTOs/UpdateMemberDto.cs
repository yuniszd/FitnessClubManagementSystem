namespace FCMS.Application.DTOs.MemberDTOs;


public record UpdateMemberDto(
    Guid Id,
    string FullName,
    string? PhoneNumber,
    string? Email
);
