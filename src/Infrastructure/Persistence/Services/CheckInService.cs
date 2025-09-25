using System.Text.Json;
using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.CheckInDTOs;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Services;

public class CheckInService : ICheckInService
{
    private readonly FitnessDbContext _context;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;

    public CheckInService(FitnessDbContext context, IRabbitMqPublisher rabbitMqPublisher)
    {
        _context = context;
        _rabbitMqPublisher = rabbitMqPublisher;
    }

    public async Task<CheckInLogDto> CheckInAsync(string cardNumber, string deviceId, CancellationToken cancellationToken = default)
    {
        // 1️⃣ Üzv tapılır
        var member = await _context.Members
            .Include(m => m.Subscriptions)
            .Include(m => m.CheckInLogs)
            .FirstOrDefaultAsync(m => m.CardNumber == cardNumber, cancellationToken);

        if (member == null)
            throw new KeyNotFoundException("Belə kart nömrəsi ilə üzv tapılmadı.");

        // 2️⃣ Aktiv abonement yoxlanılır
        var activeSubscription = member.Subscriptions
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault(s => s.IsActive);

        if (activeSubscription == null)
            throw new InvalidOperationException("Üzvün aktiv abonementi yoxdur.");

        if (activeSubscription.AllowedVisits.HasValue && activeSubscription.UsedVisits >= activeSubscription.AllowedVisits.Value)
            throw new InvalidOperationException("Bu abonement üçün icazə verilmiş ziyarət sayı bitib.");

        // 3️⃣ Duplicate check-in (1 dəqiqə window)
        var recentLog = member.CheckInLogs
            .OrderByDescending(c => c.CheckInTime)
            .FirstOrDefault();

        if (recentLog != null && (DateTime.UtcNow - recentLog.CheckInTime).TotalMinutes < 1)
            throw new InvalidOperationException("Bu üzv artıq çox tez daxil oldu.");

        // 4️⃣ Single device check (əgər device binding istifadə olunur)
        if (recentLog != null && recentLog.DeviceId != null && recentLog.DeviceId != deviceId)
            throw new InvalidOperationException("Bu QR kod başqa cihazdan istifadə edilib!");

        // 5️⃣ Check-in yaradılır
        var log = new CheckInLog
        {
            MemberId = member.Id,
            CheckInTime = DateTime.UtcNow,
            DeviceId = deviceId // Yeni property
        };

        await _context.CheckInLogs.AddAsync(log, cancellationToken);

        // 6️⃣ UsedVisits artırılır
        activeSubscription.UsedVisits++;

        await _context.SaveChangesAsync(cancellationToken);

        // 7️⃣ RabbitMQ event publish
        var eventMessage = new
        {
            MemberId = member.Id,
            MemberName = member.FullName,
            CheckInTime = log.CheckInTime,
            DeviceId = deviceId
        };
        await _rabbitMqPublisher.PublishAsync("checkin_queue", JsonSerializer.Serialize(eventMessage));

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
            throw new KeyNotFoundException("Belə check-in qeydi tapılmadı.");

        if (log.CheckOutTime != null)
            throw new InvalidOperationException("Bu check-in artıq çıxış edib.");

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
