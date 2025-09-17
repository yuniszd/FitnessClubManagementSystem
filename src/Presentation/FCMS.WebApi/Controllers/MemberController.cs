using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemberController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly ILogger<MemberController> _logger;

    public MemberController(IMemberService memberService, ILogger<MemberController> logger)
    {
        _memberService = memberService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MemberDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var members = await _memberService.GetAllAsync();

            var dtos = members.Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                PhoneNumber = m.PhoneNumber,
                Email = m.Email,
                JoinDate = m.JoinDate,
                CardNumber = m.CardNumber,
                Subscriptions = m.Subscriptions.Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    MemberId = m.Id,
                    MemberName = m.FullName,
                    SubscriptionPlanId = s.SubscriptionPlanId,
                    PlanName = s.SubscriptionPlan.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    AllowedVisits = s.AllowedVisits,
                    UsedVisits = s.UsedVisits,
                    IsActive = s.IsActive
                }).ToList()
            }).ToList();

            return Ok(new BaseResponse<IEnumerable<MemberDto>>
            {
                Success = true,
                Message = "Bütün üzvlər gətirildi",
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var member = await _memberService.GetByIdAsync(id);
            if (member == null)
                return NotFound(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Member tapılmadı"
                });

            var dto = new MemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                JoinDate = member.JoinDate,
                CardNumber = member.CardNumber,
                Subscriptions = member.Subscriptions.Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    MemberId = member.Id,
                    MemberName = member.FullName,
                    SubscriptionPlanId = s.SubscriptionPlanId,
                    PlanName = s.SubscriptionPlan.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    AllowedVisits = s.AllowedVisits,
                    UsedVisits = s.UsedVisits,
                    IsActive = s.IsActive
                }).ToList()
            };

            return Ok(new BaseResponse<MemberDto>
            {
                Success = true,
                Message = "Member tapıldı",
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetById zamanı xəta baş verdi. MemberId: {MemberId}", id);
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<MemberDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
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
            var member = await _memberService.AddMemberAsync(dto);

            var memberDto = new MemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                JoinDate = member.JoinDate,
                CardNumber = member.CardNumber,
                Subscriptions = member.Subscriptions.Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    MemberId = member.Id,
                    MemberName = member.FullName,
                    SubscriptionPlanId = s.SubscriptionPlanId,
                    PlanName = s.SubscriptionPlan.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    AllowedVisits = s.AllowedVisits,
                    UsedVisits = s.UsedVisits,
                    IsActive = s.IsActive
                }).ToList()
            };

            return CreatedAtAction(nameof(GetById), new { id = member.Id }, new BaseResponse<MemberDto>
            {
                Success = true,
                Message = "Member yaradıldı",
                Data = memberDto
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberDto dto)
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
            var member = await _memberService.GetByIdAsync(id);
            if (member == null)
                return NotFound(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Member tapılmadı"
                });

            member.FullName = dto.FullName;
            member.PhoneNumber = dto.PhoneNumber;
            member.Email = dto.Email;

            await _memberService.UpdateMemberAsync(member);

            var dtoResponse = new MemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                JoinDate = member.JoinDate,
                CardNumber = member.CardNumber,
                Subscriptions = member.Subscriptions.Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    MemberId = member.Id,
                    MemberName = member.FullName,
                    SubscriptionPlanId = s.SubscriptionPlanId,
                    PlanName = s.SubscriptionPlan.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    AllowedVisits = s.AllowedVisits,
                    UsedVisits = s.UsedVisits,
                    IsActive = s.IsActive
                }).ToList()
            };

            return Ok(new BaseResponse<MemberDto>
            {
                Success = true,
                Message = "Member yeniləndi",
                Data = dtoResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update zamanı xəta baş verdi. MemberId: {MemberId}", id);
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var member = await _memberService.GetByIdAsync(id);
            if (member == null)
                return NotFound(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Member tapılmadı"
                });

            await _memberService.DeleteMemberAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete zamanı xəta baş verdi. MemberId: {MemberId}", id);
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("by-card/{cardNumber}")]
    [ProducesResponseType(typeof(BaseResponse<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCard(string cardNumber)
    {
        try
        {
            var member = await _memberService.GetByCardAsync(cardNumber);
            if (member == null)
                return NotFound(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Member tapılmadı (kart üzrə)"
                });

            var dto = new MemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                JoinDate = member.JoinDate,
                CardNumber = member.CardNumber,
                Subscriptions = member.Subscriptions.Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    MemberId = member.Id,
                    MemberName = member.FullName,
                    SubscriptionPlanId = s.SubscriptionPlanId,
                    PlanName = s.SubscriptionPlan.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    AllowedVisits = s.AllowedVisits,
                    UsedVisits = s.UsedVisits,
                    IsActive = s.IsActive
                }).ToList()
            };

            return Ok(new BaseResponse<MemberDto>
            {
                Success = true,
                Message = "Member kart nömrəsi üzrə tapıldı",
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByCard zamanı xəta baş verdi. CardNumber: {CardNumber}", cardNumber);
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}
