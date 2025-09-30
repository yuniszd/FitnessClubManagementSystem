namespace FCMS.Application.DTOs.ReportDTOs;

public class QuickStatsRequest
{
    public DateTime? StartDate { get; set; }  // optional filter start
    public DateTime? EndDate { get; set; }    // optional filter end
}