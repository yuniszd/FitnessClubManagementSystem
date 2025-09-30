using AutoFixture;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using FCMS.Persistence.Services;
using Moq;

namespace FCMS.Tests.Services;

public class SubscriptionServiceTests
{
    private readonly Mock<IGenericRepository<Subscription>> _subscriptionRepoMock;
    private readonly Mock<IGenericRepository<Payment>> _paymentRepoMock;
    private readonly Mock<IGenericRepository<SubscriptionPlan>> _planRepoMock;
    private readonly Mock<IRabbitMqPublisher> _rabbitMock;
    private readonly Fixture _fixture;
    private readonly SubscriptionService _service;

    public SubscriptionServiceTests()
    {
        _subscriptionRepoMock = new Mock<IGenericRepository<Subscription>>();
        _paymentRepoMock = new Mock<IGenericRepository<Payment>>();
        _planRepoMock = new Mock<IGenericRepository<SubscriptionPlan>>();
        _rabbitMock = new Mock<IRabbitMqPublisher>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _service = new SubscriptionService(
            _subscriptionRepoMock.Object,
            _paymentRepoMock.Object,
            _planRepoMock.Object,
            _rabbitMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateSubscription_WhenValidDto()
    {
        // Arrange
        var dto = _fixture.Build<SubscriptionCreateDto>()
                          .With(x => x.StartDate, DateTime.UtcNow)
                          .Create();
        var plan = _fixture.Build<SubscriptionPlan>()
                           .With(p => p.DurationInMonths, 3)
                           .Create();

        _planRepoMock.Setup(r => r.GetByIdAsync(dto.SubscriptionPlanId))
                     .ReturnsAsync(plan);
        _subscriptionRepoMock.Setup(r => r.AddAsync(It.IsAny<Subscription>())).Returns(Task.CompletedTask);
        _subscriptionRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.MemberId, result.MemberId);
        Assert.Equal(dto.SubscriptionPlanId, result.SubscriptionPlanId);
        Assert.Equal(dto.StartDate.AddMonths(plan.DurationInMonths), result.EndDate);
        _subscriptionRepoMock.Verify(r => r.AddAsync(It.IsAny<Subscription>()), Times.Once);
        _subscriptionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenPlanNotFound()
    {
        var dto = _fixture.Create<SubscriptionCreateDto>();
        _planRepoMock.Setup(r => r.GetByIdAsync(dto.SubscriptionPlanId)).ReturnsAsync((SubscriptionPlan?)null);

        await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(dto));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenSubscriptionExists()
    {
        var subscription = _fixture.Create<Subscription>();
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscription.Id))
                             .ReturnsAsync(subscription);

        var result = await _service.GetByIdAsync(subscription.Id);

        Assert.NotNull(result);
        Assert.Equal(subscription.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSubscriptionNotExists()
    {
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                             .ReturnsAsync((Subscription?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSubscription_WhenValidData()
    {
        var subscription = _fixture.Create<Subscription>();
        var updateDto = new SubscriptionUpdateDto
        {
            AllowedVisits = 20,
            EndDate = DateTime.UtcNow.AddMonths(1),
            SubscriptionPlanId = subscription.SubscriptionPlanId
        };

        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscription.Id))
                             .ReturnsAsync(subscription);
        _subscriptionRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.UpdateAsync(subscription.Id, updateDto);

        Assert.NotNull(result);
        Assert.Equal(updateDto.AllowedVisits, subscription.AllowedVisits);
        Assert.Equal(updateDto.EndDate, subscription.EndDate);
        _subscriptionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenSubscriptionNotFound()
    {
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                             .ReturnsAsync((Subscription?)null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), new SubscriptionUpdateDto());
        Assert.Null(result);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenSubscriptionExists()
    {
        var subscription = _fixture.Create<Subscription>();
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscription.Id)).ReturnsAsync(subscription);
        _subscriptionRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.DeleteAsync(subscription.Id);

        Assert.True(result);
        _subscriptionRepoMock.Verify(r => r.Remove(subscription), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenSubscriptionNotFound()
    {
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Subscription?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }

    #endregion

    #region IncrementVisitAsync Tests

    [Fact]
    public async Task IncrementVisitAsync_ShouldIncreaseUsedVisits_WhenSubscriptionExists()
    {
        var subscription = _fixture.Build<Subscription>().With(s => s.UsedVisits, 5).Create();
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscription.Id)).ReturnsAsync(subscription);
        _subscriptionRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.IncrementVisitAsync(subscription.Id);

        Assert.True(result);
        Assert.Equal(6, subscription.UsedVisits);
    }

    [Fact]
    public async Task IncrementVisitAsync_ShouldReturnFalse_WhenSubscriptionNotFound()
    {
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Subscription?)null);

        var result = await _service.IncrementVisitAsync(Guid.NewGuid());
        Assert.False(result);
    }

    #endregion

    #region RenewSubscriptionAsync Tests

    [Fact]
    public async Task RenewSubscriptionAsync_ShouldAddPaymentAndExtendEndDate_AndPublishEvent()
    {
        var now = DateTime.UtcNow;
        var subscription = _fixture.Build<Subscription>()
                                   .With(s => s.StartDate, now.AddMonths(-1))
                                   .With(s => s.EndDate, now.AddMonths(1))
                                   .With(s => s.SubscriptionPlan, _fixture.Build<SubscriptionPlan>().With(p => p.DurationInMonths, 2).Create())
                                   .With(s => s.Member, _fixture.Build<Member>().With(m => m.Email, "test@test.com").With(m => m.FullName, "John Doe").Create())
                                   .Create();

        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscription.Id)).ReturnsAsync(subscription);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
        _paymentRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _subscriptionRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _rabbitMock.Setup(r => r.PublishAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        await _service.RenewSubscriptionAsync(subscription.Id, 150);

        Assert.Equal(subscription.EndDate, subscription.StartDate.AddMonths(2)); // extended
        _paymentRepoMock.Verify(r => r.AddAsync(It.Is<Payment>(p => p.Amount == 150)), Times.Once);
        _rabbitMock.Verify(r => r.PublishAsync("subscription_renewed_queue", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RenewSubscriptionAsync_ShouldThrow_WhenSubscriptionNotFound()
    {
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Subscription?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RenewSubscriptionAsync(Guid.NewGuid(), 100));
    }

    #endregion
}
