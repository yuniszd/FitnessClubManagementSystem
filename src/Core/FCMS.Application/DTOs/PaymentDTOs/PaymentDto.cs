namespace FCMS.Application.DTOs.PaymentDTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidDate { get; set; }
}
