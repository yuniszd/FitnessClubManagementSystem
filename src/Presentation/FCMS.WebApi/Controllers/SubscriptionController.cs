using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Reception")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
                                    ISubscriptionService subscriptionService,
                                    ILogger<SubscriptionsController> logger)
     {
                                    _subscriptionService = subscriptionService;
                                    _logger = logger;
     }

    #region CRUD

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<SubscriptionDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(id);
            return Ok(SuccessResponse(subscription, "Subscription uğurla tapıldı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetById zamanı xəta baş verdi");
            return NotFound(FailResponse($"Subscription tapılmadı: {ex.Message}"));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<SubscriptionDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var subscriptions = await _subscriptionService.GetAllAsync();
        return Ok(SuccessResponse(subscriptions, "Bütün subscription-lar uğurla əldə edildi"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<SubscriptionDto>), 201)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] SubscriptionCreateDto dto, [FromQuery] int? daysToAdd)
    {
        if (!ModelState.IsValid)
            return BadRequest(FailResponse("Validation xətası"));

        var subscription = await _subscriptionService.CreateAsync(dto, daysToAdd);
        return CreatedAtAction(nameof(GetById), new { id = subscription.Id },
            SuccessResponse(subscription, "Subscription uğurla yaradıldı"));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<SubscriptionDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SubscriptionUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(FailResponse("Validation xətası"));

        var updated = await _subscriptionService.UpdateAsync(id, dto);
        return Ok(SuccessResponse(updated, "Subscription uğurla yeniləndi"));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _subscriptionService.DeleteAsync(id);
        return Ok(SuccessResponse(result, "Subscription uğurla silindi"));
    }

    #endregion

    #region Extra Operations

    [HttpPost("{id:guid}/increment-visit")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    public async Task<IActionResult> IncrementVisit(Guid id)
    {
        var result = await _subscriptionService.IncrementVisitAsync(id);
        return Ok(SuccessResponse(result, "Visit sayı artırıldı"));
    }

    [HttpPost("{id:guid}/renew")]
    [ProducesResponseType(typeof(BaseResponse<string>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> Renew(Guid id, [FromQuery] decimal amountPaid, [FromQuery] int? daysToAdd)
    {
        if (amountPaid <= 0)
            return BadRequest(FailResponse("Ödəniş məbləği düzgün deyil"));

        await _subscriptionService.RenewSubscriptionAsync(id, amountPaid, daysToAdd);
        return Ok(SuccessResponse("OK", "Subscription uğurla yeniləndi"));
    }

    [HttpGet("paged")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _subscriptionService.GetPagedAsync(pageNumber, pageSize);
        return Ok(SuccessResponse(result, "Paged subscription-lar uğurla əldə edildi"));
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(BaseResponse<object>), 200)]
    public async Task<IActionResult> Search(
        [FromQuery] string? memberName,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _subscriptionService.SearchPagedAsync(memberName, isActive, pageNumber, pageSize);
        return Ok(SuccessResponse(result, "Axtarış nəticələri uğurla əldə edildi"));
    }

    #endregion

    #region Response Helpers
    private static BaseResponse<T> SuccessResponse<T>(T data, string message) =>
        new() { Success = true, Message = message, Data = data };

    private static BaseResponse<object> FailResponse(string message) =>
        new() { Success = false, Message = message };
    #endregion
}
