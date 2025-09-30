using FCMS.Application.DTOs.ReportDTOs;

namespace FCMS.Application.Abstracts;

public interface IReportService
{
    Task<ReportDto> GetAdminReportAsync();       // Admin üçün bütün statistikalar
    Task<ReportDto> GetReceptionReportAsync();   // Reception üçün sadələşdirilmiş
    Task<QuickStatsDto> GetQuickStatsAsync(QuickStatsRequest request);

}
