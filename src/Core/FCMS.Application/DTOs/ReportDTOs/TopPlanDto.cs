namespace FCMS.Application.DTOs.ReportDTOs;

public class TopPlanDto
{
    public Guid SubscriptionPlanId { get; set; }
    public string PlanName { get; set; }
    public int SubscriptionsCount { get; set; }
}
