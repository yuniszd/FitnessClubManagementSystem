using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Extensions;

public static class SubscriptionMappingExtensions
{
    // 🔹 Entity → DTO
    public static SubscriptionDto ToDto(this Subscription subscription)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        return new SubscriptionDto
        {
            Id = subscription.Id,
            MemberId = subscription.MemberId,
            MemberName = subscription.Member?.FullName ?? string.Empty,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            PlanName = subscription.SubscriptionPlan?.Name ?? string.Empty,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            AllowedVisits = subscription.AllowedVisits,
            UsedVisits = subscription.UsedVisits,
            IsActive = subscription.IsActive
        };
    }

    // 🔹 Create DTO → Entity
    public static Subscription ToEntity(this SubscriptionCreateDto dto, int? defaultDurationMonths = 1)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        return new Subscription
        {
            MemberId = dto.MemberId,
            SubscriptionPlanId = dto.SubscriptionPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(defaultDurationMonths ?? 1),
            AllowedVisits = dto.AllowedVisits,
            UsedVisits = 0
        };
    }

    // 🔹 Update DTO → Entity
    public static void UpdateFromDto(this Subscription subscription, SubscriptionUpdateDto dto)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        subscription.SubscriptionPlanId = dto.SubscriptionPlanId;
        subscription.EndDate = dto.EndDate;
        subscription.AllowedVisits = dto.AllowedVisits;
    }
}
