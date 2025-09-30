using FCMS.Application.DTOs.SubscriptionDTOs;

namespace FCMS.Application.Abstracts;

public interface ISubscriptionService
{
    Task<SubscriptionDto> GetByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionDto>> GetAllAsync();
    Task<SubscriptionDto> CreateAsync(SubscriptionCreateDto dto, int? daysToAdd = null);
    Task<SubscriptionDto> UpdateAsync(Guid id, SubscriptionUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IncrementVisitAsync(Guid id);
    Task RenewSubscriptionAsync(Guid subscriptionId, decimal amountPaid, int? daysToAdd = null);
    Task<(IEnumerable<SubscriptionDto> Subscriptions, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task<(IEnumerable<SubscriptionDto> Subscriptions, int TotalCount)> SearchPagedAsync(
        string? memberName,
        bool? isActive,
        int pageNumber,
        int pageSize);
}
