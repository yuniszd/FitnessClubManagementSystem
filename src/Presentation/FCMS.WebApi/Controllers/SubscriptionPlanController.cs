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

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var plan = await _planRepo.GetByIdAsync(id);
            return Ok(plan);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _planRepo.GetAllAsync();
            return Ok(plans);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionPlan plan)
        {
            await _planRepo.AddAsync(plan);
            await _planRepo.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = plan.Id }, plan);
        }
    }
}
