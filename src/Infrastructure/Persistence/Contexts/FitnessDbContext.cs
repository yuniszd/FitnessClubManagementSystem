using FCMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Contexts
{
    public class FitnessDbContext : IdentityDbContext<AppUser>
    {
        public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options) { }

        public DbSet<Member> Members { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CheckInLog> CheckInLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessDbContext).Assembly);
        }
    }
}
