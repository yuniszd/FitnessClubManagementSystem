using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Extensions;

public static class SubscriptionPlanMappingExtensions
{
    // 🔹 Entity → DTO
    public static SubscriptionPlanDto ToDto(this SubscriptionPlan plan)
    {
        if (plan == null) return null!;

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            DurationInMonths = plan.DurationInMonths,
            Price = plan.Price
        };
    }

    // 🔹 Create DTO → Entity
    public static SubscriptionPlan ToEntity(this SubscriptionPlanCreateDto dto)
    {
        if (dto == null) return null!;

        return new SubscriptionPlan
        {
            Name = dto.Name,
            DurationInMonths = dto.DurationInMonths,
            Price = dto.Price
        };
    }

    // 🔹 Update DTO → Entity
    public static void UpdateFromDto(this SubscriptionPlan plan, SubscriptionPlanDto dto)
    {
        if (plan == null || dto == null) return;

        plan.Name = dto.Name;
        plan.DurationInMonths = dto.DurationInMonths;
        plan.Price = dto.Price;
    }
}
