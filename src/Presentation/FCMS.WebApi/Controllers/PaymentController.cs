using FCMS.Application.Abstracts.Repositories;
using FCMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IGenericRepository<Payment> _paymentRepo;

    public PaymentController(IGenericRepository<Payment> paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var payment = await _paymentRepo.GetByIdAsync(id);
        return Ok(payment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _paymentRepo.GetAllAsync();
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Payment payment)
    {
        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = payment.Id }, payment);
    }
}
