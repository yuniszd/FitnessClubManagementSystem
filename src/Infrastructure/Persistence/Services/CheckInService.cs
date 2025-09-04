using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.CheckInDTOs;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Services;

public class CheckInService : ICheckInService
{
    private readonly FitnessDbContext _context;

    public CheckInService(FitnessDbContext context)
    {
        _context = context;
    }

    public async Task<CheckInLogDto> CheckInAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members
            .Include(m => m.Subscriptions)
            .FirstOrDefaultAsync(m => m.CardNumber == cardNumber, cancellationToken);

        if (member == null)
            throw new KeyNotFoundException($"Member with CardNumber {cardNumber} not found.");

        var activeSubscription = member.Subscriptions
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault(s => s.IsActive);

        if (activeSubscription == null)
            throw new InvalidOperationException("Member has no active subscription.");

        var log = new CheckInLog
        {
            MemberId = member.Id,
            CheckInTime = DateTime.UtcNow
        };

        await _context.CheckInLogs.AddAsync(log, cancellationToken);

        if (activeSubscription.AllowedVisits.HasValue)
            activeSubscription.UsedVisits++;

        await _context.SaveChangesAsync(cancellationToken);

        return new CheckInLogDto
        {
            Id = log.Id,
            MemberId = log.MemberId,
            CheckInTime = log.CheckInTime,
            CheckOutTime = log.CheckOutTime
        };
    }

    public async Task<CheckInLogDto> CheckOutAsync(Guid logId, CancellationToken cancellationToken = default)
    {
        var log = await _context.CheckInLogs.FindAsync(new object[] { logId }, cancellationToken);

        if (log == null)
            throw new KeyNotFoundException($"CheckInLog with Id {logId} not found.");

        if (log.CheckOutTime != null)
            throw new InvalidOperationException("This check-in already has a checkout time.");

        log.CheckOutTime = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new CheckInLogDto
        {
            Id = log.Id,
            MemberId = log.MemberId,
            CheckInTime = log.CheckInTime,
            CheckOutTime = log.CheckOutTime
        };
    }

    public async Task<IEnumerable<CheckInLogDto>> GetLogsByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        var logs = await _context.CheckInLogs
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.CheckInTime)
            .ToListAsync(cancellationToken);

        return logs.Select(log => new CheckInLogDto
        {
            Id = log.Id,
            MemberId = log.MemberId,
            CheckInTime = log.CheckInTime,
            CheckOutTime = log.CheckOutTime
        });
    }
}

