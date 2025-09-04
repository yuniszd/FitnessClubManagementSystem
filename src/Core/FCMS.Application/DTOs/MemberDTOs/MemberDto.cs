using FCMS.Application.DTOs.SubscriptionDTOs;

namespace FCMS.Application.DTOs.MemberDTOs;

public record MemberDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public DateTime JoinDate { get; init; }
    public string CardNumber { get; init; } = null!;
    public IEnumerable<SubscriptionDto> Subscriptions { get; init; } = new List<SubscriptionDto>();
}