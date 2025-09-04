namespace FCMS.Application.DTOs.SubscriptionPlanDTOs;
public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int DurationInMonths { get; set; }
    public decimal Price { get; set; }
}
