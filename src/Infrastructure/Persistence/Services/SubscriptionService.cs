using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

public class SubscriptionService : ISubscriptionService
{
    private readonly FitnessDbContext _context;

    public SubscriptionService(FitnessDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto?> GetByIdAsync(Guid id)
    {
        return await _context.Subscriptions
            .Where(s => s.Id == id)
            .Select(s => new SubscriptionDto
            {
                Id = s.Id,
                MemberId = s.MemberId,
                MemberName = s.Member != null ? s.Member.FullName : string.Empty,
                SubscriptionPlanId = s.SubscriptionPlanId,
                PlanName = s.SubscriptionPlan != null ? s.SubscriptionPlan.Name : string.Empty,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                AllowedVisits = s.AllowedVisits,
                UsedVisits = s.UsedVisits,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync();
    }



    public async Task<IEnumerable<SubscriptionDto>> GetAllAsync()
    {
        return await _context.Subscriptions
            .Select(s => new SubscriptionDto
            {
                Id = s.Id,
                MemberId = s.MemberId,
                MemberName = s.Member.FullName,
                SubscriptionPlanId = s.SubscriptionPlanId,
                PlanName = s.SubscriptionPlan.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                AllowedVisits = s.AllowedVisits,
                UsedVisits = s.UsedVisits,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<SubscriptionDto> CreateAsync(SubscriptionCreateDto dto)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            MemberId = dto.MemberId,
            SubscriptionPlanId = dto.SubscriptionPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(1), // Default 1 ay
            AllowedVisits = dto.AllowedVisits,
            UsedVisits = 0
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(subscription.Id);
    }

    public async Task<SubscriptionDto> UpdateAsync(Guid id, SubscriptionUpdateDto dto)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null) return null;

        subscription.SubscriptionPlanId = dto.SubscriptionPlanId;
        subscription.EndDate = dto.EndDate;
        subscription.AllowedVisits = dto.AllowedVisits;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(subscription.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null) return false;

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IncrementVisitAsync(Guid id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null) return false;

        if (subscription.IsActive)
        {
            subscription.UsedVisits++;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
