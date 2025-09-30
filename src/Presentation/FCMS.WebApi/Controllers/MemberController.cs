using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Application.DTOs.SubscriptionDTOs;
using FCMS.Application.Responses;
using FCMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Reception")]
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
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var members = await _memberService.GetAllAsync();
            var totalCount = members.Count();

            var dtos = members
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToMemberDto)
                .ToList();

            return Ok(new PagedResponse<MemberDto>
            {
                Success = true,
                Message = "Bütün üzvlər gətirildi",
                Data = dtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAll zamanı xəta baş verdi");
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = "Üzvlər gətirilərkən xəta baş verdi"
            });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? fullName,
        [FromQuery] string? cardNumber,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var members = await _memberService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(fullName))
                members = members.Where(m => m.FullName.Contains(fullName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(cardNumber))
                members = members.Where(m => m.CardNumber.Contains(cardNumber)).ToList();

            if (isActive.HasValue)
                members = members.Where(m => m.Subscriptions.Any(s => s.IsActive == isActive.Value)).ToList();

            var totalCount = members.Count();

            var dtos = members
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToMemberDto)
                .ToList();

            return Ok(new PagedResponse<MemberDto>
            {
                Success = true,
                Message = "Üzvlər filtrləndi",
                Data = dtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search zamanı xəta baş verdi");
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = "Üzvlərin filtrlənməsi zamanı xəta baş verdi"
            });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var member = await _memberService.GetByIdAsync(id);
            if (member == null)
                return NotFound(new BaseResponse<object> { Success = false, Message = "Member tapılmadı" });

            return Ok(new BaseResponse<MemberDto>
            {
                Success = true,
                Message = "Member tapıldı",
                Data = MapToMemberDto(member)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetById zamanı xəta baş verdi");
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = "Member tapılarkən xəta baş verdi"
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

            return CreatedAtAction(nameof(GetById), new { id = member.Id }, new BaseResponse<MemberDto>
            {
                Success = true,
                Message = "Member yaradıldı və email göndərildi",
                Data = MapToMemberDto(member)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create zamanı xəta baş verdi");
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = "Member yaradılarkən xəta baş verdi"
            });
        }
    }

    #region Private Methods
    private static MemberDto MapToMemberDto(Member member)
    {
        return new MemberDto
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
                PlanName = s.SubscriptionPlan?.Name ?? string.Empty,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                AllowedVisits = s.AllowedVisits,
                UsedVisits = s.UsedVisits,
                IsActive = s.IsActive
            }).ToList()
        };
    }
    #endregion
}
