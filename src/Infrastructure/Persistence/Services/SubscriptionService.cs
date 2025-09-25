using System.Text.Json;
using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Application.Events;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;

public class SubscriptionService : ISubscriptionService
{
    private readonly IGenericRepository<Subscription> _subscriptionRepo;
    private readonly IGenericRepository<Payment> _paymentRepo;
    private readonly IGenericRepository<SubscriptionPlan> _subscriptionPlanRepo;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;

    public SubscriptionService(
        IGenericRepository<Subscription> subscriptionRepo,
        IGenericRepository<Payment> paymentRepo,
        IGenericRepository<SubscriptionPlan> subscriptionPlanRepo,
        IRabbitMqPublisher rabbitMqPublisher)
    {
        _subscriptionRepo = subscriptionRepo;
        _paymentRepo = paymentRepo;
        _subscriptionPlanRepo = subscriptionPlanRepo;
        _rabbitMqPublisher = rabbitMqPublisher;
    }

    // ------------------- CRUD -------------------
    public async Task<SubscriptionDto> GetByIdAsync(Guid id)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(id);
        if (subscription == null) return null!;
        return MapToDto(subscription);
    }

    public async Task<IEnumerable<SubscriptionDto>> GetAllAsync()
    {
        var subscriptions = await _subscriptionRepo.GetAllAsync();
        return subscriptions.Select(MapToDto);
    }

    public async Task<SubscriptionDto> CreateAsync(SubscriptionCreateDto dto, int? daysToAdd = null)
    {
        // Planı DB-dən götür
        var plan = await _subscriptionPlanRepo.GetByIdAsync(dto.SubscriptionPlanId);
        if (plan == null) throw new Exception("Plan tapılmadı");

        // EndDate hesabla
        DateTime endDate = dto.StartDate.AddMonths(plan.DurationInMonths); // default plan duration
        if (daysToAdd.HasValue)
        {
            endDate = dto.StartDate.AddDays(daysToAdd.Value); // optional day-based duration
        }

        var subscription = new Subscription
        {
            MemberId = dto.MemberId,
            SubscriptionPlanId = dto.SubscriptionPlanId,
            StartDate = dto.StartDate,
            EndDate = endDate,
            AllowedVisits = dto.AllowedVisits
        };

        await _subscriptionRepo.AddAsync(subscription);
        await _subscriptionRepo.SaveChangesAsync();

        return MapToDto(subscription);
    }

    public async Task<SubscriptionDto> UpdateAsync(Guid id, SubscriptionUpdateDto dto)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(id);
        if (subscription == null) return null!;

        subscription.AllowedVisits = dto.AllowedVisits;
        subscription.EndDate = dto.EndDate;

        _subscriptionRepo.Update(subscription);
        await _subscriptionRepo.SaveChangesAsync();

        return MapToDto(subscription);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(id);
        if (subscription == null) return false;

        _subscriptionRepo.Remove(subscription);
        await _subscriptionRepo.SaveChangesAsync();
        return true;
    }

    // ------------------- Increment Visits -------------------
    public async Task<bool> IncrementVisitAsync(Guid id)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(id);
        if (subscription == null) return false;

        subscription.UsedVisits += 1;
        await _subscriptionRepo.SaveChangesAsync();
        return true;
    }

    // ------------------- Renew Subscription -------------------
    public async Task RenewSubscriptionAsync(Guid subscriptionId, decimal amountPaid, int? daysToAdd = null)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(subscriptionId);
        if (subscription == null)
            throw new InvalidOperationException("Abunə tapılmadı");

        var now = DateTime.UtcNow;
        var planDuration = subscription.SubscriptionPlan.DurationInMonths;

        // Ödəniş əlavə et
        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            Amount = amountPaid,
            PaidDate = now
        };
        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        // Abunə müddətini yenilə
        if (subscription.EndDate >= now)
        {
            subscription.EndDate = subscription.EndDate.AddMonths(planDuration);
        }
        else
        {
            subscription.StartDate = now;
            subscription.EndDate = now.AddMonths(planDuration);
        }

        if (daysToAdd.HasValue)
        {
            subscription.EndDate = subscription.StartDate.AddDays(daysToAdd.Value);
        }

        await _subscriptionRepo.SaveChangesAsync();

        // Event at (email/sms üçün)
        var renewalEvent = new SubscriptionRenewedEvent
        {
            Email = subscription.Member.Email!,
            FullName = subscription.Member.FullName,
            PlanName = subscription.SubscriptionPlan.Name,
            NewEndDate = subscription.EndDate
        };

        var json = JsonSerializer.Serialize(renewalEvent);
        await _rabbitMqPublisher.PublishAsync("subscription_renewed_queue", json);
    }

    // ------------------- Helper -------------------
    private SubscriptionDto MapToDto(Subscription subscription)
    {
        return new SubscriptionDto
        {
            Id = subscription.Id,
            MemberId = subscription.MemberId,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            AllowedVisits = subscription.AllowedVisits,
            UsedVisits = subscription.UsedVisits,
            IsActive = subscription.IsActive
        };
    }
}
