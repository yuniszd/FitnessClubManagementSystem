using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.CheckInDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Reception")] // yalnız reception istifadəçiləri
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromQuery] string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = "Kart nömrəsi boş ola bilməz."
            });

        try
        {
            var log = await _checkInService.CheckInAsync(cardNumber);
            return Ok(new BaseResponse<CheckInLogDto>
            {
                Success = true,
                Message = "Üzv uğurla daxil edildi.",
                Data = log
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("checkout/{logId}")]
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
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("member/{memberId}")]
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
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}
