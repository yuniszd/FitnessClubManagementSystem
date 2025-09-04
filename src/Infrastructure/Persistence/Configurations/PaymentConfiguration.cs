using FCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // Primary Key
        builder.HasKey(p => p.Id);

        // Amount sahəsi (decimal)
        builder.Property(p => p.Amount)
               .IsRequired()
               .HasPrecision(18, 2);

        // PaidDate sahəsi (DateTime)
        builder.Property(p => p.PaidDate)
               .IsRequired()
               .HasPrecision(3);

        // Subscription ilə əlaqə
        builder.HasOne(p => p.Subscription)
               .WithMany(s => s.Payments)
               .HasForeignKey(p => p.SubscriptionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
