namespace FCMS.Application.DTOs.SubscriptionPlanDTOs;

public class SubscriptionPlanCreateDto
{
    public string Name { get; set; } = null!;
    public int DurationInMonths { get; set; }
    public decimal Price { get; set; }
}