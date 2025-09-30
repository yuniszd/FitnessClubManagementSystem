using FCMS.Application.DTOs.PaymentDTOs;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.Responses;
using FCMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Reception")] 
public class PaymentController : ControllerBase
{
    private readonly IGenericRepository<Payment> _paymentRepo;

    public PaymentController(IGenericRepository<Payment> paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<PaymentDto>), 200)]
    [ProducesResponseType(typeof(BaseResponse<object>), 404)]
    public async Task<IActionResult> Get(Guid id)
    {
        var payment = await _paymentRepo.GetByIdAsync(id);
        if (payment == null)
            return NotFound(new BaseResponse<object>
            {
                Success = false,
                Message = "Ödəniş tapılmadı"
            });

        var dto = new PaymentDto
        {
            Id = payment.Id,
            SubscriptionId = payment.SubscriptionId,
            Amount = payment.Amount,
            PaidDate = payment.PaidDate
        };

        return Ok(new BaseResponse<PaymentDto>
        {
            Success = true,
            Message = "Ödəniş tapıldı",
            Data = dto
        });
    }

    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<PaymentDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _paymentRepo.GetAllAsync();
        var dtos = payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            SubscriptionId = p.SubscriptionId,
            Amount = p.Amount,
            PaidDate = p.PaidDate
        });

        return Ok(new BaseResponse<IEnumerable<PaymentDto>>
        {
            Success = true,
            Message = "Bütün ödənişlər gətirildi",
            Data = dtos
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<PaymentDto>), 201)]
    [ProducesResponseType(typeof(BaseResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] PaymentCreateDto dto)
    {
        if (dto == null)
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = "Yanlış məlumat göndərilib"
            });

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = dto.SubscriptionId,
            Amount = dto.Amount,
            PaidDate = DateTime.UtcNow
        };

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        var resultDto = new PaymentDto
        {
            Id = payment.Id,
            SubscriptionId = payment.SubscriptionId,
            Amount = payment.Amount,
            PaidDate = payment.PaidDate
        };

        return CreatedAtAction(nameof(Get), new { id = payment.Id }, new BaseResponse<PaymentDto>
        {
            Success = true,
            Message = "Ödəniş yaradıldı",
            Data = resultDto
        });
    }
}
