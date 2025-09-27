using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCMS.Application.Events;

public class CustomerRegisteredEvent
{
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string PlanName { get; set; } = default!;
    public DateTime SubscriptionEndDate { get; set; }
    public byte[]? QrCodeAttachment { get; set; } 
}

