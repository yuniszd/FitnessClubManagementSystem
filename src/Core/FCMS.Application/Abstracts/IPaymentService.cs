using FCMS.Application.DTOs.PaymentDTOs;

namespace FCMS.Application.Abstracts;

public interface IPaymentService
{
    PaymentDto CreatePayment(PaymentDto dto);
    PaymentDto GetPaymentById(Guid id);
    IEnumerable<PaymentDto> GetAllPayments();
    PaymentDto UpdatePayment(Guid id, PaymentDto dto);
    bool DeletePayment(Guid id);
}
