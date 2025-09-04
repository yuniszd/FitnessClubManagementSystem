namespace FCMS.Domain.Entities;

public class CheckInLog : BaseEntity
{
    public Guid MemberId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public Member Member { get; set; }
}
