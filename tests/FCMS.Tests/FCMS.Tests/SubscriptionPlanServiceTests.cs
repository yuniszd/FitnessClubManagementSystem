using AutoFixture;
using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using FCMS.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FCMS.Tests.Services;

public class SubscriptionPlanServiceTests
{
    private readonly Mock<FitnessDbContext> _contextMock;
    private readonly SubscriptionPlanService _service;
    private readonly Fixture _fixture;

    public SubscriptionPlanServiceTests()
    {
        _contextMock = new Mock<FitnessDbContext>();
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // DbSet mock hazırlamaq üçün helper method istifadə edəcəyik
        _service = new SubscriptionPlanService(_contextMock.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPlans()
    {
        // Arrange
        var plans = _fixture.CreateMany<SubscriptionPlan>(3).ToList();
        var dbSetMock = CreateDbSetMock(plans.AsQueryable());
        _contextMock.Setup(c => c.SubscriptionPlans).Returns(dbSetMock.Object);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.IsType<SubscriptionPlanDto>(p));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPlansExist()
    {
        var plans = new List<SubscriptionPlan>().AsQueryable();
        var dbSetMock = CreateDbSetMock(plans);
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
        // Arrange
        var dto = _fixture.Create<SubscriptionPlanCreateDto>();
        var dbSetMock = CreateDbSetMock(new List<SubscriptionPlan>().AsQueryable());
        _contextMock.Setup(c => c.SubscriptionPlans).Returns(dbSetMock.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.DurationInMonths, result.DurationInMonths);
        Assert.Equal(dto.Price, result.Price);
        _contextMock.Verify(c => c.SubscriptionPlans.Add(It.IsAny<SubscriptionPlan>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
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
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenPlanNotFound()
    {
        _contextMock.Setup(c => c.SubscriptionPlans.FindAsync(It.IsAny<Guid>())).ReturnsAsync((SubscriptionPlan?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
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
