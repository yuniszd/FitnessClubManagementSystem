using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Rola görə də filter qoyula bilər
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionsController> _logger;

        public SubscriptionsController(ISubscriptionService subscriptionService, ILogger<SubscriptionsController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        // 🔹 GET: api/subscriptions
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<SubscriptionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _subscriptionService.GetAllAsync();
                return Ok(new BaseResponse<IEnumerable<SubscriptionDto>>
                {
                    Success = true,
                    Message = "Bütün abunəliklər gətirildi",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll zamanı xəta baş verdi");
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 🔹 GET: api/subscriptions/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<SubscriptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _subscriptionService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new BaseResponse<object>
                    {
                        Success = false,
                        Message = $"Subscription tapılmadı (ID: {id})"
                    });

                return Ok(new BaseResponse<SubscriptionDto>
                {
                    Success = true,
                    Message = "Abunəlik tapıldı",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById zamanı xəta baş verdi. ID: {SubscriptionId}", id);
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 🔹 POST: api/subscriptions
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<SubscriptionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SubscriptionCreateDto dto)
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
                var created = await _subscriptionService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new BaseResponse<SubscriptionDto>
                {
                    Success = true,
                    Message = "Abunəlik yaradıldı",
                    Data = created
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create zamanı xəta baş verdi");
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 🔹 PUT: api/subscriptions/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<SubscriptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SubscriptionUpdateDto dto)
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
                var updated = await _subscriptionService.UpdateAsync(id, dto);
                if (updated == null)
                    return NotFound(new BaseResponse<object>
                    {
                        Success = false,
                        Message = $"Abunəlik tapılmadı (ID: {id})"
                    });

                return Ok(new BaseResponse<SubscriptionDto>
                {
                    Success = true,
                    Message = "Abunəlik yeniləndi",
                    Data = updated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update zamanı xəta baş verdi. ID: {SubscriptionId}", id);
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 🔹 DELETE: api/subscriptions/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _subscriptionService.DeleteAsync(id);
                if (!success)
                    return NotFound(new BaseResponse<object>
                    {
                        Success = false,
                        Message = $"Abunəlik tapılmadı (ID: {id})"
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete zamanı xəta baş verdi. ID: {SubscriptionId}", id);
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 🔹 POST: api/subscriptions/{id}/increment-visit
        [HttpPost("{id:guid}/increment-visit")]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IncrementVisit(Guid id)
        {
            try
            {
                var success = await _subscriptionService.IncrementVisitAsync(id);
                if (!success)
                    return BadRequest(new BaseResponse<object>
                    {
                        Success = false,
                        Message = "Abunəlik tapılmadı və ya aktiv deyil"
                    });

                return Ok(new BaseResponse<object>
                {
                    Success = true,
                    Message = "Visit uğurla artırıldı ✅"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IncrementVisit zamanı xəta baş verdi. ID: {SubscriptionId}", id);
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
