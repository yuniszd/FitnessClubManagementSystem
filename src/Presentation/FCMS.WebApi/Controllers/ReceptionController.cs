using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.MemberDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FCMS.WebApi.Controllers;

[Authorize(Roles = "Reception")]
[ApiController]
[Route("api/[controller]")]
public class ReceptionController : ControllerBase
{
    private readonly IMemberService _memberService;

    public ReceptionController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpPost("scan")]
    public async Task<IActionResult> ScanMember([FromBody] QrScanDto dto)
    {
        var isValid = await _memberService.ValidateQrAsync(dto.QrCode);

        if (!isValid)
            return Unauthorized(new { message = "Üzvlük etibarsızdır və ya müddəti bitib." });

        return Ok(new { message = "Üzv qəbul edildi ✅" });
    }
}

