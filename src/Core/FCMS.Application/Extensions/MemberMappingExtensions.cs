using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Extensions;
public static class MemberMappingExtensions
{
    // -----------------------------
    // CreateMemberDto → Member
    // -----------------------------
    public static Member ToEntity(this CreateMemberDto dto)
    {
        return new Member
        {
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            // CardNumber is usually generated elsewhere (QR code generation)
            // JoinDate will default to DateTime.UtcNow in Member entity
        };
    }

    // -----------------------------
    // UpdateMemberDto → Member (for updates)
    // -----------------------------
    public static void UpdateFromDto(this Member member, UpdateMemberDto dto)
    {
        member.FullName = dto.FullName;
        member.PhoneNumber = dto.PhoneNumber;
        member.Email = dto.Email;
        // Do not overwrite CardNumber or JoinDate
    }

    // -----------------------------
    // Member → MemberDto
    // -----------------------------
    public static MemberDto ToDto(this Member member)
    {
        return new MemberDto
        {
            Id = member.Id,
            FullName = member.FullName,
            PhoneNumber = member.PhoneNumber,
            Email = member.Email,
            JoinDate = member.JoinDate,
            CardNumber = member.CardNumber,
            Subscriptions = member.Subscriptions?.Select(s => s.ToDto()).ToList() ?? new List<SubscriptionDto>()
        };
    }
}