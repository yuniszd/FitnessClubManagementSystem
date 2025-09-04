using FCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StartDate)
               .IsRequired();

        builder.Property(s => s.EndDate)
               .IsRequired();

        // Relation to Member
        builder.HasOne(s => s.Member)
               .WithMany(m => m.Subscriptions)
               .HasForeignKey(s => s.MemberId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relation to SubscriptionPlan
        builder.HasOne(s => s.SubscriptionPlan)
               .WithMany(sp => sp.Subscriptions)
               .HasForeignKey(s => s.SubscriptionPlanId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relation to Payments
        builder.HasMany(s => s.Payments)
               .WithOne(p => p.Subscription)
               .HasForeignKey(p => p.SubscriptionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
