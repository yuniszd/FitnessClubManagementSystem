using FCMS.Application.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FCMS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminReport()
    {
        var report = await _reportService.GetAdminReportAsync();
        return Ok(report);
    }

    [HttpGet("reception")]
    [Authorize(Roles = "Reception")]
    public async Task<IActionResult> GetReceptionReport()
    {
        var report = await _reportService.GetReceptionReportAsync();
        return Ok(report);
    }
}



