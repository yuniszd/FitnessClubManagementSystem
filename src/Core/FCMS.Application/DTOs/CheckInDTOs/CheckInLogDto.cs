namespace FCMS.Application.DTOs.CheckInDTOs;

public record CheckInLogDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? DeviceId { get; set; } 
}
