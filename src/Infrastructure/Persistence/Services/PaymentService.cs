using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.PaymentDTOs;
using FCMS.Application.Extensions;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly FitnessDbContext _context;

        public PaymentService(FitnessDbContext context)
        {
            _context = context;
        }

        public PaymentDto CreatePayment(PaymentCreateDto dto)
        {
            if (dto == null)
                throw new ValidationException("PaymentCreateDto", "cannot be null");

            var payment = dto.ToEntity();
            _context.Payments.Add(payment);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Failed to create payment", ex);
            }

            return payment.ToDto();
        }
        
        public PaymentDto GetPaymentById(Guid id)
        {
            var payment = _context.Payments
                                  .AsNoTracking()
                                  .FirstOrDefault(p => p.Id == id);

            if (payment == null)
                throw new NotFoundException("Payment", id);

            return payment.ToDto();
        }

        public IEnumerable<PaymentDto> GetAllPayments()
        {
            return _context.Payments
                           .AsNoTracking()
                           .Select(p => p.ToDto())
                           .ToList();
        }

        public PaymentDto UpdatePayment(Guid id, PaymentDto dto)
        {
            if (dto == null)
                throw new ValidationException("PaymentDto", "cannot be null");

            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (payment == null)
                throw new NotFoundException("Payment", id);

            payment.UpdateEntity(dto);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InternalErrorException($"Failed to update payment with ID {id}", ex);
            }

            return payment.ToDto();
        }

        public void DeletePayment(Guid id)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (payment == null)
                throw new NotFoundException("Payment", id);

            _context.Payments.Remove(payment);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InternalErrorException($"Failed to delete payment with ID {id}", ex);
            }
        }
    }
}
