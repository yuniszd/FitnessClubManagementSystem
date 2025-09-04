using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Application.DTOs.SubscriptionDTOs;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemberController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MemberController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
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

        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound(new { message = "Member not found" });

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

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

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

        return CreatedAtAction(nameof(GetById), new { id = member.Id }, memberDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberDto dto)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound(new { message = "Member not found" });

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

        return Ok(dtoResponse);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound(new { message = "Member not found" });

        await _memberService.DeleteMemberAsync(id);
        return NoContent();
    }

    [HttpGet("by-card/{cardNumber}")]
    public async Task<IActionResult> GetByCard(string cardNumber)
    {
        var member = await _memberService.GetByCardAsync(cardNumber);
        if (member == null) return NotFound(new { message = "Member not found by card" });

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

        return Ok(dto);
    }
}
