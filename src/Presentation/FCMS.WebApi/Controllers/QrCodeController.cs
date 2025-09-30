using FCMS.Application.Abstracts;
using FCMS.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Reception")] 
public class QrCodeController : ControllerBase
{
    private readonly IQrCodeService _qrCodeService;
    private readonly ILogger<QrCodeController> _logger;

    public QrCodeController(IQrCodeService qrCodeService, ILogger<QrCodeController> logger)
    {
        _qrCodeService = qrCodeService;
        _logger = logger;
    }

    [HttpGet]
    [Produces("image/png")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public IActionResult Generate([FromQuery] string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(new BaseResponse<object>
            {
                Success = false,
                Message = "Content boş ola bilməz."
            });
        }

        try
        {
            var qrBytes = _qrCodeService.GenerateQrCode(content);
            return File(qrBytes, "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kod generasiya edərkən xəta baş verdi. Content: {Content}", content);
            return StatusCode(500, new BaseResponse<object>
            {
                Success = false,
                Message = "QR kod generasiya edilərkən xəta baş verdi: " + ex.Message
            });
        }
    }
}
