
using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FCMS.Persistence.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly FitnessDbContext _context;
        public SubscriptionPlanService(FitnessDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlanDto>> GetAllAsync()
        {
            return await _context.SubscriptionPlans
                .Select(sp => new SubscriptionPlanDto
                {
                    Id = sp.Id,
                    Name = sp.Name,
                    DurationInMonths = sp.DurationInMonths,
                    Price = sp.Price
                }).ToListAsync();
        }

        public async Task<SubscriptionPlanDto?> GetByIdAsync(Guid id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return null;

            return new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                DurationInMonths = plan.DurationInMonths,
                Price = plan.Price
            };
        }

        public async Task<SubscriptionPlanDto> CreateAsync(SubscriptionPlanCreateDto dto)
        {
            var plan = new SubscriptionPlan
            {
                Name = dto.Name,
                DurationInMonths = dto.DurationInMonths,
                Price = dto.Price
            };

            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();

            return new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                DurationInMonths = plan.DurationInMonths,
                Price = plan.Price
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return false;

            _context.SubscriptionPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
