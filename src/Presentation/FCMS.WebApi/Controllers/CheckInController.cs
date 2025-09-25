using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.CheckInDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Reception")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;
    private readonly ILogger<CheckInController> _logger;

    public CheckInController(ICheckInService checkInService, ILogger<CheckInController> logger)
    {
        _checkInService = checkInService;
        _logger = logger;
    }

    [HttpPost("checkin")]
    [ProducesResponseType(typeof(BaseResponse<CheckInLogDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = errors
            });
        }

        try
        {
            var log = await _checkInService.CheckInAsync(request.CardNumber, request.DeviceId);
            return Ok(new BaseResponse<CheckInLogDto>
            {
                Success = true,
                Message = "Üzv uğurla daxil edildi.",
                Data = log
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckIn zamanı xəta baş verdi. CardNumber: {CardNumber}", request.CardNumber);
            return BadRequest(new BaseResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("checkout/{logId}")]
    [ProducesResponseType(typeof(BaseResponse<CheckInLogDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CheckOut(Guid logId)
    {
        try
        {
            var log = await _checkInService.CheckOutAsync(logId);
            return Ok(new BaseResponse<CheckInLogDto>
            {
                Success = true,
                Message = "Üzv uğurla çıxış etdi.",
                Data = log
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckOut zamanı xəta baş verdi. LogId: {LogId}", logId);
            return BadRequest(new BaseResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("member/{memberId}")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<CheckInLogDto>>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> GetLogsByMember(Guid memberId)
    {
        try
        {
            var logs = await _checkInService.GetLogsByMemberAsync(memberId);
            return Ok(new BaseResponse<IEnumerable<CheckInLogDto>>
            {
                Success = true,
                Message = "Üzvün giriş/çıxış qeydləri uğurla gətirildi.",
                Data = logs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLogsByMember zamanı xəta baş verdi. MemberId: {MemberId}", memberId);
            return BadRequest(new BaseResponse<object> { Success = false, Message = ex.Message });
        }
    }
}
