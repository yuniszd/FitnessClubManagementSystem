namespace FCMS.Application.DTOs.PaymentDTOs;

public class PaymentCreateDto
{
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
}
