using FCMS.Application.DTOs.CheckInDTOs;

namespace FCMS.Application.Abstracts;

public interface ICheckInService
{
    Task<CheckInLogDto> CheckInAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task<CheckInLogDto> CheckOutAsync(Guid logId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CheckInLogDto>> GetLogsByMemberAsync(Guid memberId, CancellationToken cancellationToken = default);
}
