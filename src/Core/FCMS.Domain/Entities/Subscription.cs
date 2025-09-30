namespace FCMS.Domain.Entities;

public class Subscription : BaseEntity
{
        public Guid MemberId { get; set; }                       
        public Guid SubscriptionPlanId { get; set; }            
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Member Member { get; set; }                      
        public SubscriptionPlan SubscriptionPlan { get; set; } 
        public ICollection<Payment> Payments { get; set; } = new List<Payment>(); 
        public int? AllowedVisits { get; set; } 
        public int UsedVisits { get; set; }
        public bool IsActive => DateTime.UtcNow <= EndDate && (AllowedVisits == null || UsedVisits < AllowedVisits);

}
