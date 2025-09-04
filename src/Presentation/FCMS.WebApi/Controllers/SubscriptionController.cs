using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.SubscriptionDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // istəsən rola görə də filter qoymaq olar
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        // 🔹 GET: api/subscriptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetAll()
        {
            var result = await _subscriptionService.GetAllAsync();
            return Ok(result);
        }

        // 🔹 GET: api/subscriptions/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SubscriptionDto>> GetById(Guid id)
        {
            var result = await _subscriptionService.GetByIdAsync(id);
            if (result == null) return NotFound($"Subscription with id {id} not found.");
            return Ok(result);
        }

        // 🔹 POST: api/subscriptions
        [HttpPost]
        public async Task<ActionResult<SubscriptionDto>> Create([FromBody] SubscriptionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _subscriptionService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // 🔹 PUT: api/subscriptions/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<SubscriptionDto>> Update(Guid id, [FromBody] SubscriptionUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _subscriptionService.UpdateAsync(id, dto);
            if (updated == null) return NotFound($"Subscription with id {id} not found.");

            return Ok(updated);
        }

        // 🔹 DELETE: api/subscriptions/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _subscriptionService.DeleteAsync(id);
            if (!success) return NotFound($"Subscription with id {id} not found.");

            return NoContent();
        }

        // 🔹 POST: api/subscriptions/{id}/increment-visit
        [HttpPost("{id:guid}/increment-visit")]
        public async Task<IActionResult> IncrementVisit(Guid id)
        {
            var success = await _subscriptionService.IncrementVisitAsync(id);
            if (!success) return BadRequest("Subscription inactive or not found.");

            return Ok("Visit incremented successfully.");
        }
    }
}
