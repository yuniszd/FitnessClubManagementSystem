using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.WebApi.Controllers;

[Authorize(Roles = "Reception")]
[ApiController]
[Route("api/[controller]")]
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
        {
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = "QR kod boş ola bilməz."
            });
        }

        try
        {
            var isValid = await _memberService.ValidateQrAsync(dto.QrCode);

            if (!isValid)
            {
                return Unauthorized(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Üzvlük etibarsızdır və ya müddəti bitib."
                });
            }

            return Ok(new BaseResponse<object>
            {
                Success = true,
                Message = "Üzv qəbul edildi ✅"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ScanMember zamanı xəta baş verdi. QR: {QrCode}", dto.QrCode);
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = "Üzv skan edilərkən xəta baş verdi: " + ex.Message
            });
        }
    }
}
