using AutoFixture;
using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using FCMS.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FCMS.Application.Extensions.Logging; 
using Moq;

namespace FCMS.Tests.Services;

public class SubscriptionPlanServiceTests
{
    private readonly Mock<FitnessDbContext> _contextMock;
    private readonly Mock<ILogger<SubscriptionPlanService>> _loggerMock;
    private readonly SubscriptionPlanService _service;
    private readonly Fixture _fixture;

    public SubscriptionPlanServiceTests()
    {
        _contextMock = new Mock<FitnessDbContext>();
        _loggerMock = new Mock<ILogger<SubscriptionPlanService>>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Logger daxil edildi
        _service = new SubscriptionPlanService(_contextMock.Object, _loggerMock.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPlans()
    {
        var plans = _fixture.CreateMany<SubscriptionPlan>(3).AsQueryable();
        var dbSetMock = CreateDbSetMock(plans);
        _contextMock.Setup(c => c.SubscriptionPlans).Returns(dbSetMock.Object);

        var result = await _service.GetAllAsync();

        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.IsType<SubscriptionPlanDto>(p));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPlansExist()
    {
        var dbSetMock = CreateDbSetMock(new List<SubscriptionPlan>().AsQueryable());
        _contextMock.Setup(c => c.SubscriptionPlans).Returns(dbSetMock.Object);

        var result = await _service.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPlan_WhenExists()
    {
        var plan = _fixture.Create<SubscriptionPlan>();
        var dbSetMock = CreateDbSetMock(new List<SubscriptionPlan> { plan }.AsQueryable());
        _contextMock.Setup(c => c.SubscriptionPlans).Returns(dbSetMock.Object);
        _contextMock.Setup(c => c.SubscriptionPlans.FindAsync(plan.Id))
                    .ReturnsAsync(plan);

        var result = await _service.GetByIdAsync(plan.Id);

        Assert.NotNull(result);
        Assert.Equal(plan.Id, result.Id);
        Assert.Equal(plan.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPlanNotFound()
    {
        _contextMock.Setup(c => c.SubscriptionPlans.FindAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((SubscriptionPlan?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldAddPlanAndReturnDto()
    {
        var dto = _fixture.Create<SubscriptionPlanCreateDto>();
        var dbSetMock = CreateDbSetMock(new List<SubscriptionPlan>().AsQueryable());
        _contextMock.Setup(c => c.SubscriptionPlans).Returns(dbSetMock.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.DurationInMonths, result.DurationInMonths);
        Assert.Equal(dto.Price, result.Price);

        _contextMock.Verify(c => c.SubscriptionPlans.Add(It.IsAny<SubscriptionPlan>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once()); // helper extension lazım ola bilər
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenPlanExists()
    {
        var plan = _fixture.Create<SubscriptionPlan>();
        var dbSetMock = CreateDbSetMock(new List<SubscriptionPlan> { plan }.AsQueryable());
        _contextMock.Setup(c => c.SubscriptionPlans).Returns(dbSetMock.Object);
        _contextMock.Setup(c => c.SubscriptionPlans.FindAsync(plan.Id)).ReturnsAsync(plan);
        _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.DeleteAsync(plan.Id);

        Assert.True(result);
        _contextMock.Verify(c => c.SubscriptionPlans.Remove(plan), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFound_WhenPlanNotFound()
    {
        _contextMock.Setup(c => c.SubscriptionPlans.FindAsync(It.IsAny<Guid>())).ReturnsAsync((SubscriptionPlan?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid()));

        _contextMock.Verify(c => c.SubscriptionPlans.Remove(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    #endregion

    #region Helper Methods

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
