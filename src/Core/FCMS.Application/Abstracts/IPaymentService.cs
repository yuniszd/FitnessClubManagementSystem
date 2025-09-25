using FCMS.Application.DTOs.PaymentDTOs;

namespace FCMS.Application.Abstracts;

public interface IPaymentService
{
    // Yeni ödəniş yaratmaq
    PaymentDto CreatePayment(PaymentCreateDto dto);

    // ID ilə ödənişi götürmək
    PaymentDto? GetPaymentById(Guid id);

    // Bütün ödənişləri gətirmək
    IEnumerable<PaymentDto> GetAllPayments();

    // Ödənişi yeniləmək
    PaymentDto? UpdatePayment(Guid id, PaymentDto dto);

    // Ödənişi silmək
    bool DeletePayment(Guid id);
}
