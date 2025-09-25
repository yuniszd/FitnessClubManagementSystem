using FCMS.Application.DTOs.PaymentDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Extensions;

public static class PaymentMappingExtensions
{
    // CreateDto → Entity
    public static Payment ToEntity(this PaymentCreateDto dto)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            Amount = dto.Amount,
            PaidDate = DateTime.UtcNow,
            SubscriptionId = dto.SubscriptionId
        };
    }

    // Entity → DTO
    public static PaymentDto ToDto(this Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaidDate = payment.PaidDate,
            SubscriptionId = payment.SubscriptionId
        };
    }

    // DTO → Entity (Update zamanı istifadə oluna bilər)
    public static void UpdateEntity(this Payment payment, PaymentDto dto)
    {
        payment.Amount = dto.Amount;
        payment.PaidDate = dto.PaidDate;
        payment.SubscriptionId = dto.SubscriptionId;
    }
}