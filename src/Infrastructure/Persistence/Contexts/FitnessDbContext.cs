using FCMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Contexts;

    public class FitnessDbContext : IdentityDbContext<IdentityUser>
    {
        public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options) { }

        // Müştəri və digər cədvəllər
        public DbSet<Member> Members { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CheckInLog> CheckInLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ⚠️ Əvvəl Identity üçün base çağırılmalıdır
            base.OnModelCreating(modelBuilder);

            // Fluent API config-lərin avtomatik tətbiqi
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessDbContext).Assembly);
        }
    }

