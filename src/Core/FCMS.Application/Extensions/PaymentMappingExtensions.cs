using FCMS.Application.DTOs.PaymentDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Extensions;

public static class PaymentMappingExtensions
{
    // DTO → Entity
    public static Payment ToEntity(this PaymentDto dto)
    {
        return new Payment
        {
            Id = dto.Id,
            Amount = dto.Amount,
            PaidDate = dto.PaidDate,
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
}
