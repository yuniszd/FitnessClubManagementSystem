namespace FCMS.Application.DTOs.SubscriptionDTOs;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string MemberName { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string PlanName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? AllowedVisits { get; set; }
    public int UsedVisits { get; set; }
    public bool IsActive { get; set; }
}