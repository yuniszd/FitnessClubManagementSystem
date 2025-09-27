using AutoFixture;
using FCMS.Application.DTOs.PaymentDTOs;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using FCMS.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FCMS.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<FitnessDbContext> _contextMock;
    private readonly PaymentService _service;
    private readonly Fixture _fixture;

    public PaymentServiceTests()
    {
        _contextMock = new Mock<FitnessDbContext>();
        _service = new PaymentService(_contextMock.Object);
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    #region CreatePayment Tests

    [Fact]
    public void CreatePayment_ShouldCreatePayment_WhenValidDtoProvided()
    {
        // Arrange
        var dto = _fixture.Build<PaymentCreateDto>()
                          .With(x => x.Amount, 150)
                          .With(x => x.SubscriptionId, Guid.NewGuid())
                          .Create();

        var payments = new List<Payment>().AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);

        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);
        _contextMock.Setup(c => c.SaveChanges()).Returns(1);

        // Act
        var result = _service.CreatePayment(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Amount, result.Amount);
        Assert.Equal(dto.SubscriptionId, result.SubscriptionId);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.PaidDate <= DateTime.UtcNow);

        _contextMock.Verify(c => c.Payments.Add(It.IsAny<Payment>()), Times.Once);
        _contextMock.Verify(c => c.SaveChanges(), Times.Once);
    }

    [Fact]
    public void CreatePayment_ShouldThrow_WhenDtoIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CreatePayment(null!));
    }

    #endregion

    #region GetPaymentById Tests

    [Fact]
    public void GetPaymentById_ShouldReturnPayment_WhenExists()
    {
        var payment = _fixture.Build<Payment>()
                              .With(x => x.Id, Guid.NewGuid())
                              .With(x => x.Amount, 200)
                              .Create();

        var payments = new List<Payment> { payment }.AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);

        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);

        var result = _service.GetPaymentById(payment.Id);

        Assert.NotNull(result);
        Assert.Equal(payment.Id, result.Id);
        Assert.Equal(payment.Amount, result.Amount);
    }

    [Fact]
    public void GetPaymentById_ShouldReturnNull_WhenNotExists()
    {
        var payments = new List<Payment>().AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);
        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);

        var result = _service.GetPaymentById(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public void GetPaymentById_ShouldReturnNull_WhenIdIsEmpty()
    {
        var payments = new List<Payment>().AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);
        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);

        var result = _service.GetPaymentById(Guid.Empty);
        Assert.Null(result);
    }

    #endregion

    #region GetAllPayments Tests

    [Fact]
    public void GetAllPayments_ShouldReturnAllPayments_WhenPaymentsExist()
    {
        var payments = _fixture.CreateMany<Payment>(5).ToList();
        var dbSetMock = CreateDbSetMock(payments.AsQueryable());

        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);

        var result = _service.GetAllPayments().ToList();

        Assert.Equal(5, result.Count);
        Assert.All(result, r => Assert.IsType<PaymentDto>(r));
    }

    [Fact]
    public void GetAllPayments_ShouldReturnEmpty_WhenNoPaymentsExist()
    {
        var payments = new List<Payment>().AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);

        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);

        var result = _service.GetAllPayments();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region UpdatePayment Tests

    [Fact]
    public void UpdatePayment_ShouldUpdate_WhenPaymentExists()
    {
        var payment = _fixture.Build<Payment>()
                              .With(x => x.Id, Guid.NewGuid())
                              .With(x => x.Amount, 100)
                              .Create();

        var payments = new List<Payment> { payment }.AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);

        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);
        _contextMock.Setup(c => c.SaveChanges()).Returns(1);

        var updateDto = new PaymentDto
        {
            Id = payment.Id,
            SubscriptionId = payment.SubscriptionId,
            Amount = 500,
            PaidDate = DateTime.UtcNow
        };

        var result = _service.UpdatePayment(payment.Id, updateDto);

        Assert.NotNull(result);
        Assert.Equal(updateDto.Amount, result.Amount);
        Assert.Equal(updateDto.PaidDate, result.PaidDate);
        _contextMock.Verify(c => c.SaveChanges(), Times.Once);
    }

    [Fact]
    public void UpdatePayment_ShouldReturnNull_WhenPaymentNotExists()
    {
        var payments = new List<Payment>().AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);

        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);

        var result = _service.UpdatePayment(Guid.NewGuid(), new PaymentDto());
        Assert.Null(result);
        _contextMock.Verify(c => c.SaveChanges(), Times.Never);
    }

    #endregion

    #region DeletePayment Tests

    [Fact]
    public void DeletePayment_ShouldReturnTrue_WhenPaymentExists()
    {
        var payment = _fixture.Build<Payment>()
                              .With(x => x.Id, Guid.NewGuid())
                              .Create();

        var payments = new List<Payment> { payment }.AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);

        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);
        _contextMock.Setup(c => c.SaveChanges()).Returns(1);

        var result = _service.DeletePayment(payment.Id);

        Assert.True(result);
        _contextMock.Verify(c => c.Payments.Remove(payment), Times.Once);
        _contextMock.Verify(c => c.SaveChanges(), Times.Once);
    }

    [Fact]
    public void DeletePayment_ShouldReturnFalse_WhenPaymentNotExists()
    {
        var payments = new List<Payment>().AsQueryable();
        var dbSetMock = CreateDbSetMock(payments);

        _contextMock.Setup(c => c.Payments).Returns(dbSetMock.Object);

        var result = _service.DeletePayment(Guid.NewGuid());

        Assert.False(result);
        _contextMock.Verify(c => c.Payments.Remove(It.IsAny<Payment>()), Times.Never);
    }

    #endregion

    #region Helper Method

    private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }

    #endregion
}
