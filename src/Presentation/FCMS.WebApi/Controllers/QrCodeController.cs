using FCMS.Application.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace FCMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrCodeController : ControllerBase
    {
        private readonly IQrCodeService _qrCodeService;

        public QrCodeController(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        // GET: api/qrcode?content=ABC123
        [HttpGet]
        public IActionResult Generate(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Content cannot be empty.");

            var qrBytes = _qrCodeService.GenerateQrCode(content);
            return File(qrBytes, "image/png");
        }
    }
}
