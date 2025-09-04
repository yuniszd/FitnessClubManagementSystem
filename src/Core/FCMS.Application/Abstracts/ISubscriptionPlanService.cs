using FCMS.Application.DTOs.SubscriptionPlanDTOs;

namespace FCMS.Application.Abstracts;

public interface ISubscriptionPlanService
{
    Task<List<SubscriptionPlanDto>> GetAllAsync();
    Task<SubscriptionPlanDto?> GetByIdAsync(Guid id);
    Task<SubscriptionPlanDto> CreateAsync(SubscriptionPlanCreateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
