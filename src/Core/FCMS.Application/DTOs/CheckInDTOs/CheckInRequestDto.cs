using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCMS.Application.DTOs.CheckInDTOs;

public record CheckInRequestDto
{
    public string CardNumber { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty; 
}
