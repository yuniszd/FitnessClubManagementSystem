namespace FCMS.Application.DTOs.SubscriptionDTOs;

public class SubscriptionUpdateDto
{
    public Guid SubscriptionPlanId { get; set; }
    public DateTime EndDate { get; set; }
    public int? AllowedVisits { get; set; }
}
