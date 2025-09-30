using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Reception")]
public class ReceptionController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly ILogger<ReceptionController> _logger;

    public ReceptionController(IMemberService memberService, ILogger<ReceptionController> logger)
    {
        _memberService = memberService;
        _logger = logger;
    }

    [HttpPost("scan")]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ScanMember([FromBody] QrScanDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.QrCode))
            return BadRequest(FailResponse("QR kod boş ola bilməz."));

        try
        {
            var isValid = await _memberService.ValidateQrAsync(dto.QrCode);

            if (!isValid)
                return Unauthorized(FailResponse("Üzvlük etibarsızdır və ya müddəti bitib."));

            return Ok(SuccessResponse<object?>(null, "Üzv qəbul edildi ✅"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ScanMember zamanı xəta baş verdi. QR: {QrCode}", dto.QrCode);
            return StatusCode(500, FailResponse("Üzv skan edilərkən xəta baş verdi: " + ex.Message));
        }
    }

    #region Response Helpers
    private static BaseResponse<T?> SuccessResponse<T>(T? data, string message) =>
        new() { Success = true, Message = message, Data = data };

    private static BaseResponse<object> FailResponse(string message) =>
        new() { Success = false, Message = message };
    #endregion
}
