using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Application.Events;
using FCMS.Application.Extensions;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using System.Text.Json;

namespace FCMS.Persistence.Services;

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
        var subscription = await _subscriptionRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Subscription", id);

        return subscription.ToDto();
    }

    public async Task<IEnumerable<SubscriptionDto>> GetAllAsync()
    {
        var subscriptions = await _subscriptionRepo.GetAllAsync();
        return subscriptions.Select(s => s.ToDto());
    }

    public async Task<SubscriptionDto> CreateAsync(SubscriptionCreateDto dto, int? daysToAdd = null)
    {
        var errors = new List<string>();

        if (dto == null)
        {
            errors.Add("SubscriptionCreateDto cannot be null");
            throw new ValidationException(errors.ToArray());
        }

        if (dto.MemberId == Guid.Empty)
            errors.Add("MemberId is required");

        if (dto.SubscriptionPlanId == Guid.Empty)
            errors.Add("SubscriptionPlanId is required");

        if (dto.StartDate == default)
            errors.Add("StartDate is required");

        if (dto.AllowedVisits.HasValue && dto.AllowedVisits < 0)
            errors.Add("AllowedVisits cannot be negative");

        if (errors.Any())
            throw new ValidationException(errors.ToArray());

        var plan = await _subscriptionPlanRepo.GetByIdAsync(dto.SubscriptionPlanId)
            ?? throw new NotFoundException("SubscriptionPlan", dto.SubscriptionPlanId);

        DateTime endDate = dto.StartDate.AddMonths(plan.DurationInMonths);
        if (daysToAdd.HasValue)
            endDate = dto.StartDate.AddDays(daysToAdd.Value);

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

        return subscription.ToDto();
    }

    public async Task<SubscriptionDto> UpdateAsync(Guid id, SubscriptionUpdateDto dto)
    {
        var errors = new List<string>();
        if (dto == null)
            errors.Add("SubscriptionUpdateDto cannot be null");

        if (dto.AllowedVisits.HasValue && dto.AllowedVisits < 0)
            errors.Add("AllowedVisits cannot be negative");

        if (errors.Any())
            throw new ValidationException(errors.ToArray());

        var subscription = await _subscriptionRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Subscription", id);

        subscription.UpdateFromDto(dto);

        _subscriptionRepo.Update(subscription);
        await _subscriptionRepo.SaveChangesAsync();

        return subscription.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Subscription", id);

        _subscriptionRepo.Remove(subscription);
        await _subscriptionRepo.SaveChangesAsync();

        return true;
    }

    // ------------------- Increment Visits -------------------
    public async Task<bool> IncrementVisitAsync(Guid id)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Subscription", id);

        subscription.UsedVisits += 1;
        await _subscriptionRepo.SaveChangesAsync();

        return true;
    }

    // ------------------- Renew Subscription -------------------
    public async Task RenewSubscriptionAsync(Guid subscriptionId, decimal amountPaid, int? daysToAdd = null)
    {
        var subscription = await _subscriptionRepo.GetByIdAsync(subscriptionId)
            ?? throw new NotFoundException("Subscription", subscriptionId);

        var now = DateTime.UtcNow;
        var planDuration = subscription.SubscriptionPlan.DurationInMonths;

        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            Amount = amountPaid,
            PaidDate = now
        };

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

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
            subscription.EndDate = subscription.StartDate.AddDays(daysToAdd.Value);

        await _subscriptionRepo.SaveChangesAsync();

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

    // ------------------- Paging & Search -------------------
    public async Task<(IEnumerable<SubscriptionDto> Subscriptions, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _subscriptionRepo.GetQueryable();
        int totalCount = query.Count();
        var data = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return await Task.FromResult((data.Select(s => s.ToDto()), totalCount));
    }

    public async Task<(IEnumerable<SubscriptionDto> Subscriptions, int TotalCount)> SearchPagedAsync(
        string? memberName, bool? isActive, int pageNumber, int pageSize)
    {
        var query = _subscriptionRepo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(memberName))
            query = query.Where(s => s.Member.FullName.Contains(memberName));

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        int totalCount = query.Count();
        var data = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return await Task.FromResult((data.Select(s => s.ToDto()), totalCount));
    }
}
