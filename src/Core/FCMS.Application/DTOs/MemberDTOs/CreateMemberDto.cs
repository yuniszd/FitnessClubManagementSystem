namespace FCMS.Application.DTOs.MemberDTOs;

public class CreateMemberDto
{
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    // Subscription plan seçimi
    public Guid SubscriptionPlanId { get; set; }
    public int? AllowedVisits { get; set; } // optional
}
