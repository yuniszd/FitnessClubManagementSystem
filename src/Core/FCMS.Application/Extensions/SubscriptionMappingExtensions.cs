using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Extensions;

public static class SubscriptionMappingExtensions
{
    // 🔹 Entity → DTO
    public static SubscriptionDto ToDto(this Subscription subscription)
    {
        if (subscription == null) return null!;

        return new SubscriptionDto
        {
            Id = subscription.Id,
            MemberId = subscription.MemberId,
            MemberName = subscription.Member?.FullName ?? string.Empty,  // navigation property
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            PlanName = subscription.SubscriptionPlan?.Name ?? string.Empty,  // navigation property
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            AllowedVisits = subscription.AllowedVisits,
            UsedVisits = subscription.UsedVisits,
            IsActive = subscription.IsActive
        };
    }

    // 🔹 Create DTO → Entity
    public static Subscription ToEntity(this SubscriptionCreateDto dto)
    {
        if (dto == null) return null!;

        return new Subscription
        {
            MemberId = dto.MemberId,
            SubscriptionPlanId = dto.SubscriptionPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(1), // default 1 aylıq subscription, lazım olsa dəyiş
            AllowedVisits = dto.AllowedVisits,
            UsedVisits = 0
        };
    }

    // 🔹 Update DTO → Entity
    public static void UpdateFromDto(this Subscription subscription, SubscriptionUpdateDto dto)
    {
        if (subscription == null || dto == null) return;

        subscription.SubscriptionPlanId = dto.SubscriptionPlanId;
        subscription.EndDate = dto.EndDate;
        subscription.AllowedVisits = dto.AllowedVisits;
        // UsedVisits dəyişmir, yalnız admin update edə bilər
    }
}
