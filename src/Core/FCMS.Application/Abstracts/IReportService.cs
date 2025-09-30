using FCMS.Application.DTOs.ReportDTOs;

namespace FCMS.Application.Abstracts;

public interface IReportService
{
    Task<ReportDto> GetAdminReportAsync();       
    Task<ReportDto> GetReceptionReportAsync();  
    Task<QuickStatsDto> GetQuickStatsAsync(QuickStatsRequest request);

}
