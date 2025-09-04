using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.PaymentDTOs;
using FCMS.Application.Extensions;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
namespace FCMS.Persistence.Services;

public class PaymentService : IPaymentService
{
    private readonly FitnessDbContext _context;

    public PaymentService(FitnessDbContext context)
    {
        _context = context;
    }

    // Yeni ödəniş yaratmaq
    public PaymentDto CreatePayment(PaymentDto dto)
    {
        var payment = dto.ToEntity();
        _context.Payments.Add(payment);
        _context.SaveChanges();
        return payment.ToDto();
    }

    // ID ilə ödənişi götürmək
    public PaymentDto GetPaymentById(Guid id)
    {
        var payment = _context.Payments
                              .AsNoTracking()
                              .FirstOrDefault(p => p.Id == id);
        return payment?.ToDto();
    }

    // Bütün ödənişləri gətirmək
    public IEnumerable<PaymentDto> GetAllPayments()
    {
        return _context.Payments
                       .AsNoTracking()
                       .Select(p => p.ToDto())
                       .ToList();
    }

    // Ödənişi yeniləmək
    public PaymentDto UpdatePayment(Guid id, PaymentDto dto)
    {
        var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
        if (payment == null) return null;

        payment.Amount = dto.Amount;
        payment.PaidDate = dto.PaidDate;
        payment.SubscriptionId = dto.SubscriptionId;

        _context.SaveChanges();
        return payment.ToDto();
    }

    // Ödənişi silmək
    public bool DeletePayment(Guid id)
    {
        var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
        if (payment == null) return false;

        _context.Payments.Remove(payment);
        _context.SaveChanges();
        return true;
    }
}
