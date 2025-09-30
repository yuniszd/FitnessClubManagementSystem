using FCMS.Application.DTOs.CheckInDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Application.Extensions;

public static class CheckInMappingExtensions
{
    // 🔹 Entity → DTO
    public static CheckInLogDto ToDto(this CheckInLog log)
    {
        if (log == null) return null!;

        return new CheckInLogDto
        {
            Id = log.Id,
            MemberId = log.MemberId,
            CheckInTime = log.CheckInTime,
            CheckOutTime = log.CheckOutTime,
            DeviceId = log.DeviceId
        };
    }

    // 🔹 Request DTO → Entity
    public static CheckInLog ToEntity(this CheckInRequestDto dto, Guid memberId)
    {
        if (dto == null) return null!;

        return new CheckInLog
        {
            MemberId = memberId,
            CheckInTime = DateTime.UtcNow,
            DeviceId = dto.DeviceId
        };
    }

    // 🔹 Optional: Update CheckOutTime
    public static void SetCheckOut(this CheckInLog log)
    {
        if (log == null) return;

        log.CheckOutTime = DateTime.UtcNow;
    }
}
