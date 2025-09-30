using AutoFixture;
using FCMS.Application.DTOs.PaymentDTOs;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using FCMS.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FCMS.Tests.Services
{
    public class PaymentServiceTests : IDisposable
    {
        private readonly FitnessDbContext _context;
        private readonly PaymentService _service;
        private readonly Fixture _fixture;

        public PaymentServiceTests()
        {
            var options = new DbContextOptionsBuilder<FitnessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new FitnessDbContext(options);
            _service = new PaymentService(_context);

            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region CreatePayment

        [Fact]
        public void CreatePayment_ShouldAddPayment_WhenValidDto()
        {
            var dto = _fixture.Build<PaymentCreateDto>()
                              .With(x => x.Amount, 100)
                              .With(x => x.SubscriptionId, Guid.NewGuid())
                              .Create();

            var result = _service.CreatePayment(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Amount, result.Amount);
            Assert.Equal(dto.SubscriptionId, result.SubscriptionId);
            Assert.True(result.PaidDate <= DateTime.UtcNow);

            var paymentInDb = _context.Payments.FirstOrDefault(p => p.Id == result.Id);
            Assert.NotNull(paymentInDb);
        }

        [Fact]
        public void CreatePayment_ShouldThrow_WhenDtoIsNull()
        {
            Assert.Throws<ValidationException>(() => _service.CreatePayment(null!));
        }

        #endregion

        #region GetPaymentById

        [Fact]
        public void GetPaymentById_ShouldReturnPayment_WhenExists()
        {
            var payment = _fixture.Create<Payment>();
            _context.Payments.Add(payment);
            _context.SaveChanges();

            var result = _service.GetPaymentById(payment.Id);

            Assert.NotNull(result);
            Assert.Equal(payment.Id, result.Id);
        }

        [Fact]
        public void GetPaymentById_ShouldThrowNotFound_WhenNotExists()
        {
            var id = Guid.NewGuid();
            Assert.Throws<NotFoundException>(() => _service.GetPaymentById(id));
        }

        #endregion

        #region GetAllPayments

        [Fact]
        public void GetAllPayments_ShouldReturnAllPayments()
        {
            var payments = _fixture.CreateMany<Payment>(3).ToList();
            _context.Payments.AddRange(payments);
            _context.SaveChanges();

            var result = _service.GetAllPayments().ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAllPayments_ShouldReturnEmpty_WhenNoPayments()
        {
            var result = _service.GetAllPayments();
            Assert.Empty(result);
        }

        #endregion

        #region UpdatePayment

        [Fact]
        public void UpdatePayment_ShouldModifyPayment_WhenExists()
        {
            var payment = _fixture.Create<Payment>();
            _context.Payments.Add(payment);
            _context.SaveChanges();

            var dto = new PaymentDto
            {
                Id = payment.Id,
                Amount = 999,
                SubscriptionId = payment.SubscriptionId,
                PaidDate = DateTime.UtcNow
            };

            var result = _service.UpdatePayment(payment.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(999, result.Amount);

            var updated = _context.Payments.Find(payment.Id);
            Assert.Equal(999, updated!.Amount);
        }

        [Fact]
        public void UpdatePayment_ShouldThrowNotFound_WhenPaymentNotExists()
        {
            var dto = _fixture.Create<PaymentDto>();
            Assert.Throws<NotFoundException>(() => _service.UpdatePayment(Guid.NewGuid(), dto));
        }

        #endregion

        #region DeletePayment

        [Fact]
        public void DeletePayment_ShouldRemovePayment_WhenExists()
        {
            var payment = _fixture.Create<Payment>();
            _context.Payments.Add(payment);
            _context.SaveChanges();

            _service.DeletePayment(payment.Id);

            var deleted = _context.Payments.Find(payment.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public void DeletePayment_ShouldThrowNotFound_WhenPaymentNotExists()
        {
            Assert.Throws<NotFoundException>(() => _service.DeletePayment(Guid.NewGuid()));
        }

        #endregion
    }
}
