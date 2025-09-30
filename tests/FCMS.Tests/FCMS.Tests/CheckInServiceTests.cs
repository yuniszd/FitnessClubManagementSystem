using AutoFixture;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using FCMS.Persistence.Contexts;
using FCMS.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCMS.Tests;

public class CheckInServiceTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly Mock<IRabbitMqPublisher> _rabbitMqPublisherMock;
    private readonly Mock<ILogger<CheckInService>> _loggerMock;
    private readonly DbContextOptions<FitnessDbContext> _dbOptions;

    public CheckInServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _rabbitMqPublisherMock = new Mock<IRabbitMqPublisher>();
        _loggerMock = new Mock<ILogger<CheckInService>>();

        _dbOptions = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    public void Dispose()
    {
        using var context = new FitnessDbContext(_dbOptions);
        context.Database.EnsureDeleted();
    }

    private CheckInService CreateService(FitnessDbContext context)
        => new CheckInService(context, _rabbitMqPublisherMock.Object, _loggerMock.Object);

    #region CheckInAsync Tests

    [Fact]
    public async Task CheckInAsync_InvalidCard_ShouldThrowNotFound()
    {
        using var context = new FitnessDbContext(_dbOptions);
        var service = CreateService(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CheckInAsync("INVALID_CARD", "DEVICE1"));
    }

    [Fact]
    public async Task CheckInAsync_NoActiveSubscription_ShouldThrowBusinessRule()
    {
        using var context = new FitnessDbContext(_dbOptions);
        var member = new Member
        {
            Id = Guid.NewGuid(),
            CardNumber = "CARD123",
            Subscriptions = new List<Subscription>()
        };
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CheckInAsync("CARD123", "DEVICE1"));
    }

    [Fact]
    public async Task CheckInAsync_ExhaustedVisits_ShouldThrowBusinessRule()
    {
        using var context = new FitnessDbContext(_dbOptions);
        var subscription = new Subscription
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            AllowedVisits = 5,
            UsedVisits = 5
        };
        var member = new Member
        {
            Id = Guid.NewGuid(),
            CardNumber = "CARD123",
            Subscriptions = new List<Subscription> { subscription }
        };
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CheckInAsync("CARD123", "DEVICE1"));
    }

    [Fact]
    public async Task CheckInAsync_Valid_ShouldCreateLogIncrementVisitsPublishEvent()
    {
        using var context = new FitnessDbContext(_dbOptions);
        var subscription = new Subscription
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            AllowedVisits = 10,
            UsedVisits = 2
        };
        var member = new Member
        {
            Id = Guid.NewGuid(),
            CardNumber = "CARD123",
            FullName = "Test User",
            Subscriptions = new List<Subscription> { subscription },
            CheckInLogs = new List<CheckInLog>()
        };
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.CheckInAsync("CARD123", "DEVICE1");

        Assert.NotNull(result);
        Assert.Equal(member.Id, result.MemberId);

        var logInDb = await context.CheckInLogs.FirstOrDefaultAsync(l => l.Id == result.Id);
        Assert.NotNull(logInDb);
        Assert.Equal("DEVICE1", logInDb.DeviceId);
        Assert.Equal(3, subscription.UsedVisits);

        _rabbitMqPublisherMock.Verify(r => r.PublishAsync("checkin_queue", It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region CheckOutAsync Tests

    [Fact]
    public async Task CheckOutAsync_InvalidLog_ShouldThrowNotFound()
    {
        using var context = new FitnessDbContext(_dbOptions);
        var service = CreateService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CheckOutAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CheckOutAsync_Valid_ShouldSetCheckOutTime()
    {
        using var context = new FitnessDbContext(_dbOptions);
        var log = new CheckInLog { CheckInTime = DateTime.UtcNow.AddHours(-1) };
        context.CheckInLogs.Add(log);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.CheckOutAsync(log.Id);

        Assert.NotNull(result.CheckOutTime);

        var logInDb = await context.CheckInLogs.FindAsync(log.Id);
        Assert.NotNull(logInDb.CheckOutTime);
    }

    #endregion
}
