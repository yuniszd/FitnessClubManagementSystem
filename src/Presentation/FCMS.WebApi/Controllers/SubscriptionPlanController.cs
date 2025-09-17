using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FCMS.Domain.Entities;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly IGenericRepository<SubscriptionPlan> _planRepo;
        private readonly ILogger<SubscriptionPlanController> _logger;

        public SubscriptionPlanController(IGenericRepository<SubscriptionPlan> planRepo, ILogger<SubscriptionPlanController> logger)
        {
            _planRepo = planRepo;
            _logger = logger;
        }

        // GET /api/subscriptionplan
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<SubscriptionPlanDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var plans = await _planRepo.GetAllAsync();

                var dtos = plans.Select(plan => new SubscriptionPlanDto
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    DurationInMonths = plan.DurationInMonths,
                    Price = plan.Price
                }).ToList();

                return Ok(new BaseResponse<IEnumerable<SubscriptionPlanDto>>
                {
                    Success = true,
                    Message = "Bütün abunə planları gətirildi",
                    Data = dtos
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

        // GET /api/subscriptionplan/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<SubscriptionPlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var plan = await _planRepo.GetByIdAsync(id);
                if (plan == null)
                    return NotFound(new BaseResponse<object>
                    {
                        Success = false,
                        Message = $"Subscription plan tapılmadı (ID: {id})"
                    });

                var dto = new SubscriptionPlanDto
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    DurationInMonths = plan.DurationInMonths,
                    Price = plan.Price
                };

                return Ok(new BaseResponse<SubscriptionPlanDto>
                {
                    Success = true,
                    Message = "Abunə planı tapıldı",
                    Data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById zamanı xəta baş verdi. ID: {PlanId}", id);
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // POST /api/subscriptionplan
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<SubscriptionPlanDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SubscriptionPlanCreateDto dto)
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
                var plan = new SubscriptionPlan
                {
                    Name = dto.Name,
                    DurationInMonths = dto.DurationInMonths,
                    Price = dto.Price
                };

                await _planRepo.AddAsync(plan);
                await _planRepo.SaveChangesAsync();

                var responseDto = new SubscriptionPlanDto
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    DurationInMonths = plan.DurationInMonths,
                    Price = plan.Price
                };

                return CreatedAtAction(nameof(GetById), new { id = plan.Id }, new BaseResponse<SubscriptionPlanDto>
                {
                    Success = true,
                    Message = "Abunə planı yaradıldı",
                    Data = responseDto
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

        // PUT /api/subscriptionplan/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BaseResponse<SubscriptionPlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] SubscriptionPlanCreateDto dto)
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
                var plan = await _planRepo.GetByIdAsync(id);
                if (plan == null)
                    return NotFound(new BaseResponse<object>
                    {
                        Success = false,
                        Message = $"Subscription plan tapılmadı (ID: {id})"
                    });

                plan.Name = dto.Name;
                plan.DurationInMonths = dto.DurationInMonths;
                plan.Price = dto.Price;

                await _planRepo.SaveChangesAsync();

                var responseDto = new SubscriptionPlanDto
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    DurationInMonths = plan.DurationInMonths,
                    Price = plan.Price
                };

                return Ok(new BaseResponse<SubscriptionPlanDto>
                {
                    Success = true,
                    Message = "Abunə planı yeniləndi",
                    Data = responseDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update zamanı xəta baş verdi. ID: {PlanId}", id);
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // DELETE /api/subscriptionplan/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var plan = await _planRepo.GetByIdAsync(id);
                if (plan == null)
                    return NotFound(new BaseResponse<object>
                    {
                        Success = false,
                        Message = $"Subscription plan tapılmadı (ID: {id})"
                    });

                _planRepo.Remove(plan);
                await _planRepo.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete zamanı xəta baş verdi. ID: {PlanId}", id);
                return StatusCode(500, new BaseResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
