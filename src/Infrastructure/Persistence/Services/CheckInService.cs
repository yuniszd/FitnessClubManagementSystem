using System.Text.Json;
using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.CheckInDTOs;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCMS.Persistence.Services;

public class CheckInService : ICheckInService
{
    private readonly FitnessDbContext _context;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly ILogger<CheckInService> _logger;

    public CheckInService(
        FitnessDbContext context,
        IRabbitMqPublisher rabbitMqPublisher,
        ILogger<CheckInService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _rabbitMqPublisher = rabbitMqPublisher ?? throw new ArgumentNullException(nameof(rabbitMqPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CheckInLogDto> CheckInAsync(string cardNumber, string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new ValidationException("cardNumber", "Card number is required");

        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ValidationException("deviceId", "Device ID is required");

        var member = await _context.Members
            .Include(m => m.Subscriptions)
            .Include(m => m.CheckInLogs)
            .FirstOrDefaultAsync(m => m.CardNumber == cardNumber && !m.IsDeleted, cancellationToken);

        if (member == null)
        {
            _logger.LogWarning("Check-in failed: member not found for card {CardNumber}", cardNumber);
            throw new NotFoundException("Member", cardNumber);
        }

        var activeSubscription = member.Subscriptions
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault(s => s.IsActive);

        if (activeSubscription == null)
        {
            _logger.LogWarning("Check-in failed: no active subscription for member {MemberId}", member.Id);
            throw new BusinessRuleException("Üzvün aktiv abonementi yoxdur");
        }

        var lastLog = member.CheckInLogs
            .OrderByDescending(l => l.CheckInTime)
            .FirstOrDefault();

        if (lastLog != null && lastLog.CheckOutTime == null)
        {
            lastLog.CheckOutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Member {MemberId} automatically checked out previous session", member.Id);

            return MapToDto(lastLog);
        }

        var newLog = new CheckInLog
        {
            MemberId = member.Id,
            CheckInTime = DateTime.UtcNow,
            DeviceId = deviceId
        };

        await _context.CheckInLogs.AddAsync(newLog, cancellationToken);

        if (activeSubscription.AllowedVisits.HasValue)
            activeSubscription.UsedVisits++;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Member {MemberId} checked in successfully", member.Id);

        try
        {
            var eventMessage = new
            {
                MemberId = member.Id,
                MemberName = member.FullName,
                CheckInTime = newLog.CheckInTime,
                DeviceId = deviceId
            };
            await _rabbitMqPublisher.PublishAsync("checkin_queue", JsonSerializer.Serialize(eventMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish check-in event for member {MemberId}", member.Id);
        }

        return MapToDto(newLog);
    }

    public async Task<CheckInLogDto> CheckOutAsync(Guid logId, CancellationToken cancellationToken = default)
    {
        var log = await _context.CheckInLogs
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == logId, cancellationToken);

        if (log == null)
        {
            _logger.LogWarning("Check-out failed: log not found {LogId}", logId);
            throw new NotFoundException("Check-in log", logId);
        }

        if (log.CheckOutTime != null)
        {
            _logger.LogWarning("Check-out failed: log already checked out {LogId}", logId);
            throw new BusinessRuleException("Bu check-in artıq çıxış edib");
        }

        log.CheckOutTime = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Member {MemberId} checked out successfully", log.MemberId);

        return MapToDto(log);
    }

    public async Task<IEnumerable<CheckInLogDto>> GetLogsByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        var logs = await _context.CheckInLogs
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.CheckInTime)
            .ToListAsync(cancellationToken);

        return logs.Select(MapToDto);
    }

    #region Private Helpers
    private static CheckInLogDto MapToDto(CheckInLog log)
    {
        return new CheckInLogDto
        {
            Id = log.Id,
            MemberId = log.MemberId,
            CheckInTime = log.CheckInTime,
            CheckOutTime = log.CheckOutTime,
            DeviceId = log.DeviceId
        };
    }
    #endregion
}
