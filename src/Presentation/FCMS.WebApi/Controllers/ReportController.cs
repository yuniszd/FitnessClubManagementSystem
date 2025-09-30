using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.ReportDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 500)]
    public async Task<IActionResult> GetAdminReport()
    {
        try
        {
            var report = await _reportService.GetAdminReportAsync();
            return Ok(SuccessResponse(report, "Admin report uğurla əldə edildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAdminReport zamanı xəta baş verdi");
            return StatusCode(500, FailResponse("Admin report əldə edilərkən xəta baş verdi: " + ex.Message));
        }
    }

    [HttpGet("reception")]
    [Authorize(Roles = "Reception")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 500)]
    public async Task<IActionResult> GetReceptionReport()
    {
        try
        {
            var report = await _reportService.GetReceptionReportAsync();
            return Ok(SuccessResponse(report, "Reception report uğurla əldə edildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetReceptionReport zamanı xəta baş verdi");
            return StatusCode(500, FailResponse("Reception report əldə edilərkən xəta baş verdi: " + ex.Message));
        }
    }

    // ---------------- Quick Stats ----------------
    [HttpGet("quick-stats")]
    [Authorize(Roles = "Admin,Reception")]
    [ProducesResponseType(typeof(BaseResponse<QuickStatsDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 500)]
    public async Task<IActionResult> GetQuickStats([FromQuery] QuickStatsRequest request)
    {
        try
        {
            var stats = await _reportService.GetQuickStatsAsync(request);
            return Ok(SuccessResponse(stats, "Quick stats uğurla əldə edildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetQuickStats zamanı xəta baş verdi");
            return StatusCode(500, FailResponse("Quick stats əldə edilərkən xəta baş verdi: " + ex.Message));
        }
    }

    #region Response Helpers
    private static BaseResponse<T> SuccessResponse<T>(T data, string message) =>
        new() { Success = true, Message = message, Data = data };

    private static BaseResponse<object> FailResponse(string message) =>
        new() { Success = false, Message = message };
    #endregion
}
