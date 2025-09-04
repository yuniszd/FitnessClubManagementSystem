using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.CheckInDTOs;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        // POST: api/checkin?cardNumber=12345
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromQuery] string cardNumber)
        {
            var log = await _checkInService.CheckInAsync(cardNumber);
            return Ok(log);
        }

        // POST: api/checkin/checkout/{logId}
        [HttpPost("checkout/{logId}")]
        public async Task<IActionResult> CheckOut(Guid logId)
        {
            var log = await _checkInService.CheckOutAsync(logId);
            return Ok(log);
        }

        // GET: api/checkin/member/{memberId}
        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetLogsByMember(Guid memberId)
        {
            var logs = await _checkInService.GetLogsByMemberAsync(memberId);
            return Ok(logs);
        }
    }
}
