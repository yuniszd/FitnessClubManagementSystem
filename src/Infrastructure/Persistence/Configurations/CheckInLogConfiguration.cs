using FCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Configurations;

public class CheckInLogConfiguration : IEntityTypeConfiguration<CheckInLog>
{
    public void Configure(EntityTypeBuilder<CheckInLog> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CheckInTime)
               .IsRequired();

        builder.Property(c => c.CheckOutTime);

        builder.HasOne(c => c.Member)
               .WithMany(m => m.CheckInLogs)
               .HasForeignKey(c => c.MemberId)
               .OnDelete(DeleteBehavior.Cascade); // member silindikdə loglar da silinsin
    }
}
