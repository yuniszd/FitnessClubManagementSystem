using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.ReportDTOs;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Services;

public class ReportService : IReportService
{
    private readonly FitnessDbContext _context;

    public ReportService(FitnessDbContext context)
    {
        _context = context;
    }

    // ------------------ Admin Report ------------------
    public async Task<ReportDto> GetAdminReportAsync()
    {
        // Toplam üzvlər
        var totalMembers = await _context.Members.CountAsync();

        // Aktiv üzvlər (subscription hələ də aktiv)
        var activeMembers = await _context.Subscriptions
            .Where(s => s.EndDate >= DateTime.UtcNow &&
                        (s.AllowedVisits == null || s.UsedVisits < s.AllowedVisits))
            .Select(s => s.MemberId)
            .Distinct()
            .CountAsync();

        // Bu ayın gəliri
        var monthlyRevenue = await _context.Payments
            .Where(p => p.PaidDate.Month == DateTime.UtcNow.Month &&
                        p.PaidDate.Year == DateTime.UtcNow.Year)
            .SumAsync(p => p.Amount);

        // Top 5 subscription plan
        var topPlans = await _context.Subscriptions
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

    // ------------------ Reception Report ------------------
    public async Task<ReportDto> GetReceptionReportAsync()
    {
        // Aktiv üzvlər
        var activeMembers = await _context.Subscriptions
            .Where(s => s.EndDate >= DateTime.UtcNow &&
                        (s.AllowedVisits == null || s.UsedVisits < s.AllowedVisits))
            .Select(s => s.MemberId)
            .Distinct()
            .CountAsync();

        // Bu ayın gəliri
        var monthlyRevenue = await _context.Payments
            .Where(p => p.PaidDate.Month == DateTime.UtcNow.Month &&
                        p.PaidDate.Year == DateTime.UtcNow.Year)
            .SumAsync(p => p.Amount);

        // Reception üçün top plan-lar lazım deyil
        return new ReportDto
        {
            TotalMembers = 0, // admin görə bilir
            ActiveMembers = activeMembers,
            MonthlyRevenue = monthlyRevenue,
            TopPlans = new List<TopPlanDto>()
        };
    }
}
