using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FCMS.Domain.Entities;
using FCMS.Application.Abstracts.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly IGenericRepository<SubscriptionPlan> _planRepo;

        public SubscriptionPlanController(IGenericRepository<SubscriptionPlan> planRepo)
        {
            _planRepo = planRepo;
        }

        // GET /api/subscriptionplan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _planRepo.GetAllAsync();

            var dtos = plans.Select(plan => new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                DurationInMonths = plan.DurationInMonths,
                Price = plan.Price
            }).ToList();

            return Ok(dtos);
        }

        // GET /api/subscriptionplan/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plan = await _planRepo.GetByIdAsync(id);
            if (plan == null) return NotFound();

            var dto = new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                DurationInMonths = plan.DurationInMonths,
                Price = plan.Price
            };

            return Ok(dto);
        }

        // POST /api/subscriptionplan
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionPlanCreateDto dto)
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

            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, responseDto);
        }

        // PUT /api/subscriptionplan/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SubscriptionPlanCreateDto dto)
        {
            var plan = await _planRepo.GetByIdAsync(id);
            if (plan == null) return NotFound();

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

            return Ok(responseDto);
        }

        // DELETE /api/subscriptionplan/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var plan = await _planRepo.GetByIdAsync(id);
            if (plan == null) return NotFound();

            _planRepo.Remove(plan);
            await _planRepo.SaveChangesAsync();

            return NoContent();
        }
    }
}
