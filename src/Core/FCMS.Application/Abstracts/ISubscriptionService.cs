using FCMS.Application.DTOs.SubscriptionDTOs;

namespace FCMS.Application.Abstracts;

public interface ISubscriptionService
{
    Task<SubscriptionDto> GetByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionDto>> GetAllAsync();
    Task<SubscriptionDto> CreateAsync(SubscriptionCreateDto dto);
    Task<SubscriptionDto> UpdateAsync(Guid id, SubscriptionUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IncrementVisitAsync(Guid id);  // hər gəlişdə UsedVisits artırmaq üçün
}
