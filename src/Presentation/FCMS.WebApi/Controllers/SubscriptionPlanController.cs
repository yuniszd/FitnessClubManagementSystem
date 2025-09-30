using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FCMS.Domain.Entities;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FCMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly IGenericRepository<SubscriptionPlan> _planRepo;
        private readonly ILogger<SubscriptionPlanController> _logger;

        public SubscriptionPlanController(IGenericRepository<SubscriptionPlan> planRepo, ILogger<SubscriptionPlanController> logger)
        {
            _planRepo = planRepo;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
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

                return Ok(SuccessResponse(dtos, "Bütün abunə planları gətirildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll zamanı xəta baş verdi");
                return StatusCode(500, FailResponse(ex.Message));
            }
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var plan = await _planRepo.GetByIdAsync(id);
                if (plan == null)
                    return NotFound(FailResponse($"Subscription plan tapılmadı (ID: {id})"));

                var dto = new SubscriptionPlanDto
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    DurationInMonths = plan.DurationInMonths,
                    Price = plan.Price
                };

                return Ok(SuccessResponse(dto, "Abunə planı tapıldı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById zamanı xəta baş verdi. ID: {PlanId}", id);
                return StatusCode(500, FailResponse(ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionPlanCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(FailResponse(ModelState));

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

                return CreatedAtAction(nameof(GetById), new { id = plan.Id }, SuccessResponse(responseDto, "Abunə planı yaradıldı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create zamanı xəta baş verdi");
                return StatusCode(500, FailResponse(ex.Message));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SubscriptionPlanCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(FailResponse(ModelState));

            try
            {
                var plan = await _planRepo.GetByIdAsync(id);
                if (plan == null)
                    return NotFound(FailResponse($"Subscription plan tapılmadı (ID: {id})"));

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

                return Ok(SuccessResponse(responseDto, "Abunə planı yeniləndi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update zamanı xəta baş verdi. ID: {PlanId}", id);
                return StatusCode(500, FailResponse(ex.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var plan = await _planRepo.GetByIdAsync(id);
                if (plan == null)
                    return NotFound(FailResponse($"Subscription plan tapılmadı (ID: {id})"));

                _planRepo.Remove(plan);
                await _planRepo.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete zamanı xəta baş verdi. ID: {PlanId}", id);
                return StatusCode(500, FailResponse(ex.Message));
            }
        }

        #region Response Helpers
        private static BaseResponse<T?> SuccessResponse<T>(T? data, string message) =>
            new() { Success = true, Message = message, Data = data };

        private static BaseResponse<object> FailResponse(string message) =>
            new() { Success = false, Message = message };

        private static BaseResponse<object> FailResponse(ModelStateDictionary modelState)
        {
            var errors = string.Join("; ", modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return new BaseResponse<object> { Success = false, Message = errors };
        }
        #endregion
    }
}
