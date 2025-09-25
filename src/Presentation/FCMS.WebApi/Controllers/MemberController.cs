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

    [HttpPost]
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
                Message = "Member yaradıldı və email göndərildi",
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null)
            return NotFound(new BaseResponse<object> { Success = false, Message = "Member tapılmadı" });

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

    // Digər metodlar: Update, Delete, GetByCard dəyişmir, köhnə kodu istifadə edə bilərsən
}
