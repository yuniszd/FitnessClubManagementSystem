namespace FCMS.Application.DTOs.SubscriptionDTOs;

public class SubscriptionCreateDto
{
    public Guid MemberId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public int? AllowedVisits { get; set; }
}
