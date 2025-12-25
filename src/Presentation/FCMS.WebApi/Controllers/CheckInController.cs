using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.CheckInDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

    #region DTOs

    public record CheckInRequest([Required] string CardNumber, [Required] string DeviceId);

    #endregion

    [HttpPost("checkin")]
    [ProducesResponseType(typeof(BaseResponse<CheckInLogDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(FailResponse("CardNumber və DeviceId tələb olunur", ModelState));

        try
        {
            var log = await _checkInService.CheckInAsync(request.CardNumber, request.DeviceId);

            var message = log.CheckOutTime != null
                ? "Üzv uğurla çıxış etdi."
                : "Üzv uğurla daxil edildi.";

            return Ok(SuccessResponse(log, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckIn/CheckOut zamanı xəta baş verdi. CardNumber: {CardNumber}", request.CardNumber);
            return BadRequest(FailResponse(ex.Message));
        }
    }


    [HttpPost("checkout/{logId:guid}")]
    [ProducesResponseType(typeof(BaseResponse<CheckInLogDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> CheckOut(Guid logId)
    {
        try
        {
            var log = await _checkInService.CheckOutAsync(logId);
            return Ok(SuccessResponse(log, "Üzv uğurla çıxış etdi."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckOut zamanı xəta baş verdi. LogId: {LogId}", logId);
            return BadRequest(FailResponse(ex.Message));
        }
    }

    [HttpGet("member/{memberId:guid}")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<CheckInLogDto>>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> GetLogsByMember(Guid memberId)
    {
        try
        {
            var logs = await _checkInService.GetLogsByMemberAsync(memberId);
            return Ok(SuccessResponse(logs, "Üzvün giriş/çıxış qeydləri uğurla gətirildi."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLogsByMember zamanı xəta baş verdi. MemberId: {MemberId}", memberId);
            return BadRequest(FailResponse(ex.Message));
        }
    }

    #region Response Helpers

    private static BaseResponse<T> SuccessResponse<T>(T data, string message) =>
        new() { Success = true, Message = message, Data = data };

    private static BaseResponse<object> FailResponse(string message) =>
        new() { Success = false, Message = message };

    private static BaseResponse<object> FailResponse(string message, ModelStateDictionary modelState)
    {
        var errors = string.Join("; ", modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        return new BaseResponse<object> { Success = false, Message = $"{message}: {errors}" };
    }

    #endregion
}
