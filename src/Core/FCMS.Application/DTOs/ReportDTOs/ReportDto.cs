using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCMS.Application.DTOs.ReportDTOs;

public class ReportDto
{
    public int TotalMembers { get; set; }           // Bütün üzvlər
    public int ActiveMembers { get; set; }          // Aktiv üzvlər
    public decimal MonthlyRevenue { get; set; }     // Bu ayın ödənişləri
    public List<TopPlanDto> TopPlans { get; set; }  // Ən çox seçilən planlar
}
