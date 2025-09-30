using FCMS.Application.DTOs.ReportDTOs;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using FCMS.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCMS.Tests;

public class ReportServiceTests
{
    private readonly FitnessDbContext _context;
    private readonly ReportService _service;
    private readonly Mock<ILogger<ReportService>> _loggerMock;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FitnessDbContext(options);
        _loggerMock = new Mock<ILogger<ReportService>>();
        _service = new ReportService(_context, _loggerMock.Object);

        SeedData();
    }

    private void SeedData()
    {
        // Subscription plan
        var plan = new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Gold", DurationInMonths = 1, Price = 100 };
        _context.SubscriptionPlans.Add(plan);

        // Members
        var member1 = new Member { FullName = "Alice", JoinDate = DateTime.UtcNow.AddDays(-10) };
        var member2 = new Member { FullName = "Bob", JoinDate = DateTime.UtcNow.AddDays(-5) };
        var member3 = new Member { FullName = "Charlie", JoinDate = DateTime.UtcNow };

        _context.Members.AddRange(member1, member2, member3);

        // Subscriptions
        var subscription1 = new Subscription { Member = member1, SubscriptionPlan = plan, StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(5), AllowedVisits = 10, UsedVisits = 3 };
        var subscription2 = new Subscription { Member = member2, SubscriptionPlan = plan, StartDate = DateTime.UtcNow.AddDays(-4), EndDate = DateTime.UtcNow.AddDays(6), AllowedVisits = 10, UsedVisits = 3 };
        var subscription3 = new Subscription { Member = member3, SubscriptionPlan = plan, StartDate = DateTime.UtcNow.AddDays(-3), EndDate = DateTime.UtcNow.AddDays(7), AllowedVisits = 10, UsedVisits = 3 };

        _context.Subscriptions.AddRange(subscription1, subscription2, subscription3);

        // Payments
        var payment1 = new Payment { Subscription = subscription1, Amount = 100, PaidDate = DateTime.UtcNow };
        var payment2 = new Payment { Subscription = subscription2, Amount = 100, PaidDate = DateTime.UtcNow };
        var payment3 = new Payment { Subscription = subscription3, Amount = 100, PaidDate = DateTime.UtcNow };

        _context.Payments.AddRange(payment1, payment2, payment3);

        // Check-in logs
        var checkIn1 = new CheckInLog { Member = member1, CheckInTime = DateTime.UtcNow };
        var checkIn2 = new CheckInLog { Member = member2, CheckInTime = DateTime.UtcNow };
        var checkIn3 = new CheckInLog { Member = member3, CheckInTime = DateTime.UtcNow };

        _context.CheckInLogs.AddRange(checkIn1, checkIn2, checkIn3);

        _context.SaveChanges();
    }

    #region Admin Report Tests
    [Fact]
    public async Task GetAdminReportAsync_ShouldReturnCorrectData()
    {
        var result = await _service.GetAdminReportAsync();

        Assert.Equal(3, result.TotalMembers);
        Assert.Equal(3, result.ActiveMembers);
        Assert.Equal(300, result.MonthlyRevenue);
        Assert.Single(result.TopPlans);
        Assert.Equal("Gold", result.TopPlans.First().PlanName);
    }
    #endregion

    #region Reception Report Tests
    [Fact]
    public async Task GetReceptionReportAsync_ShouldReturnCorrectData()
    {
        var result = await _service.GetReceptionReportAsync();

        Assert.Equal(3, result.ActiveMembers);
        Assert.Equal(300, result.MonthlyRevenue);
        Assert.Empty(result.TopPlans);
    }
    #endregion

    #region Quick Stats Tests
    [Fact]
    public async Task GetQuickStatsAsync_ShouldReturnCorrectCounts_WithDateRange()
    {
        var request = new QuickStatsRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        var result = await _service.GetQuickStatsAsync(request);

        Assert.Equal(3, result.TodayCheckIns);
        Assert.Equal(2, result.NewMembersThisMonth); // 2 join within last 7 days
        Assert.Equal(3, result.ExpiringSubscriptions); // All subscriptions end within next 7 days
    }

    [Fact]
    public async Task GetQuickStatsAsync_ShouldReturnCorrectCounts_WithoutDateRange()
    {
        var request = new QuickStatsRequest(); // null dates => default to today

        var result = await _service.GetQuickStatsAsync(request);

        Assert.Equal(3, result.TodayCheckIns);
        Assert.Equal(1, result.NewMembersThisMonth); // Joined today
        Assert.Equal(3, result.ExpiringSubscriptions);
    }
    #endregion
}
