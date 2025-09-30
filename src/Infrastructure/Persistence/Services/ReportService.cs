using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.ReportDTOs;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCMS.Persistence.Services;

public class ReportService : IReportService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(FitnessDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ------------------ Admin Report ------------------
    public async Task<ReportDto> GetAdminReportAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            var totalMembers = await _context.Members.CountAsync();

            var activeMembers = await _context.Subscriptions
                .Where(s => s.EndDate >= now &&
                            (s.AllowedVisits == null || s.UsedVisits < s.AllowedVisits))
                .Select(s => s.MemberId)
                .Distinct()
                .CountAsync();

            var monthlyRevenue = await _context.Payments
                .Where(p => p.PaidDate.Month == now.Month &&
                            p.PaidDate.Year == now.Year)
                .SumAsync(p => p.Amount);

            var topPlans = await _context.Subscriptions
                .Where(s => s.SubscriptionPlan != null)
                .GroupBy(s => new { s.SubscriptionPlanId, s.SubscriptionPlan.Name })
                .Select(g => new TopPlanDto
                {
                    SubscriptionPlanId = g.Key.SubscriptionPlanId,
                    PlanName = g.Key.Name,
                    SubscriptionsCount = g.Count()
                })
                .OrderByDescending(p => p.SubscriptionsCount)
                .Take(5)
                .ToListAsync();

            return new ReportDto
            {
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                MonthlyRevenue = monthlyRevenue,
                TopPlans = topPlans
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate admin report");
            throw new InternalErrorException("Failed to generate admin report", ex);
        }
    }

    // ------------------ Reception Report ------------------
    public async Task<ReportDto> GetReceptionReportAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            var activeMembers = await _context.Subscriptions
                .Where(s => s.EndDate >= now &&
                            (s.AllowedVisits == null || s.UsedVisits < s.AllowedVisits))
                .Select(s => s.MemberId)
                .Distinct()
                .CountAsync();

            var monthlyRevenue = await _context.Payments
                .Where(p => p.PaidDate.Month == now.Month &&
                            p.PaidDate.Year == now.Year)
                .SumAsync(p => p.Amount);

            return new ReportDto
            {
                TotalMembers = 0,
                ActiveMembers = activeMembers,
                MonthlyRevenue = monthlyRevenue,
                TopPlans = new List<TopPlanDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate reception report");
            throw new InternalErrorException("Failed to generate reception report", ex);
        }
    }

    // ------------------ Quick Stats ------------------
    public async Task<QuickStatsDto> GetQuickStatsAsync(QuickStatsRequest? request = null)
    {
        try
        {
            var now = DateTime.UtcNow;
            var start = request?.StartDate ?? now.Date;
            var end = request?.EndDate ?? now.Date;

            var todayCheckIns = await _context.CheckInLogs
                .CountAsync(c => c.CheckInTime.Date >= start.Date && c.CheckInTime.Date <= end.Date);


            var newMembers = await _context.Members
                .CountAsync(m => m.JoinDate.Date >= start.Date &&
                                 m.JoinDate.Date <= end.Date &&
                                 !m.IsDeleted);

            var expiringSubs = await _context.Subscriptions
                .Where(s => s.EndDate.Date >= start.Date &&
                            s.EndDate.Date <= end.Date)
                .CountAsync();

            return new QuickStatsDto
            {
                TodayCheckIns = todayCheckIns,
                NewMembersThisMonth = newMembers,
                ExpiringSubscriptions = expiringSubs
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate quick stats");
            throw new InternalErrorException("Failed to generate quick stats", ex);
        }
    }
}
