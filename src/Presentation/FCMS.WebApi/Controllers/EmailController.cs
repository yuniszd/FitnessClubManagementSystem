using FCMS.Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FCMS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IRabbitMqPublisher _publisher;

    public EmailController(IRabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailMessage message)
    {
        await _publisher.PublishAsync("email_queue", System.Text.Json.JsonSerializer.Serialize(message));
        return Ok(new { status = "queued", message.Subject });
    }
}
