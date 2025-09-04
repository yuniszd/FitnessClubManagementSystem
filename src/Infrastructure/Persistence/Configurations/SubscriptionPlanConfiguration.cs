using FCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(sp => sp.DurationInMonths)
               .IsRequired();

        builder.Property(sp => sp.Price)
               .IsRequired()
               .HasPrecision(18, 2); 

        builder.HasMany(sp => sp.Subscriptions)
               .WithOne(s => s.SubscriptionPlan)
               .HasForeignKey(s => s.SubscriptionPlanId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}