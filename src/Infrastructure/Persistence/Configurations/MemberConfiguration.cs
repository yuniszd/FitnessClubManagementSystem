using FCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FullName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(m => m.CardNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasMany(m => m.Subscriptions)
               .WithOne(s => s.Member)
               .HasForeignKey(s => s.MemberId);

        builder.HasMany(m => m.CheckInLogs)
               .WithOne(c => c.Member)
               .HasForeignKey(c => c.MemberId);
    }
}
