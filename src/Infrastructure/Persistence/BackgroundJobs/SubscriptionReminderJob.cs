namespace FCMS.Persistence.BackgroundJobs;

using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.Events;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class SubscriptionReminderJob
{
    private readonly IGenericRepository<Subscription> _subscriptionRepo;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;

    public SubscriptionReminderJob(
        IGenericRepository<Subscription> subscriptionRepo,
        IRabbitMqPublisher rabbitMqPublisher)
    {
        _subscriptionRepo = subscriptionRepo;
        _rabbitMqPublisher = rabbitMqPublisher;
    }

    public async Task SendRemindersAsync()
    {
        var now = DateTime.UtcNow;

        var subscriptions = await _subscriptionRepo
            .GetQueryable()
            .Include(s => s.Member)
            .Include(s => s.SubscriptionPlan)
            .ToListAsync();

        foreach (var sub in subscriptions)
        {
            var remainingVisits = sub.AllowedVisits.HasValue
                ? sub.AllowedVisits.Value - sub.UsedVisits
                : (int?)null;

            bool isLastVisits = remainingVisits.HasValue &&
                                remainingVisits.Value <= 3 &&
                                remainingVisits.Value > 0; 

            var daysLeft = (sub.EndDate - now).TotalDays;
            bool isLastDay = daysLeft <= 1 && daysLeft >= 0;

            if (isLastVisits || isLastDay)
            {
                if (sub.Member == null || sub.SubscriptionPlan == null)
                    continue;

                var reminderEvent = new SubscriptionReminderEvent
                {
                    Email = sub.Member.Email!,
                    FullName = sub.Member.FullName,
                    PlanName = sub.SubscriptionPlan.Name,
                    RemainingVisits = remainingVisits,
                    DaysLeft = (int)Math.Ceiling(daysLeft)
                };

                var json = JsonSerializer.Serialize(reminderEvent);
                await _rabbitMqPublisher.PublishAsync("subscription_reminder_queue", json);
            }
        }
    }
}
