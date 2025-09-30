using FCMS.Application.Abstracts;
using FCMS.Application.DTOs.SubscriptionPlanDTOs;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Domain.Entities;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCMS.Persistence.Services;

public class SubscriptionPlanService : ISubscriptionPlanService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<SubscriptionPlanService> _logger;

    public SubscriptionPlanService(FitnessDbContext context, ILogger<SubscriptionPlanService> logger)
    {
        _context = context;
        _logger = logger;
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
        if (dto == null)
            throw new ValidationException(new[] { "SubscriptionPlanCreateDto cannot be null" });

        var validationErrors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Name))
            validationErrors.Add("Subscription plan name is required");

        if (dto.DurationInMonths <= 0)
            validationErrors.Add("DurationInMonths must be greater than zero");

        if (dto.Price < 0)
            validationErrors.Add("Price cannot be negative");

        if (validationErrors.Any())
            throw new ValidationException(validationErrors.ToArray());

        var plan = new SubscriptionPlan
        {
            Name = dto.Name,
            DurationInMonths = dto.DurationInMonths,
            Price = dto.Price
        };

        try
        {
            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("SubscriptionPlan created: {PlanId}", plan.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create subscription plan");
            throw new InternalErrorException("Failed to create subscription plan", ex);
        }

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            DurationInMonths = plan.DurationInMonths,
            Price = plan.Price
        };
    }

    public async Task<SubscriptionPlanDto> UpdateAsync(Guid id, SubscriptionPlanCreateDto dto)
    {
        if (dto == null)
            throw new ValidationException(new[] { "SubscriptionPlanCreateDto cannot be null" });

        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null)
            throw new NotFoundException("SubscriptionPlan", id);

        var validationErrors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Name))
            validationErrors.Add("Subscription plan name is required");

        if (dto.DurationInMonths <= 0)
            validationErrors.Add("DurationInMonths must be greater than zero");

        if (dto.Price < 0)
            validationErrors.Add("Price cannot be negative");

        if (validationErrors.Any())
            throw new ValidationException(validationErrors.ToArray());

        plan.Name = dto.Name;
        plan.DurationInMonths = dto.DurationInMonths;
        plan.Price = dto.Price;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("SubscriptionPlan updated: {PlanId}", plan.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update subscription plan {PlanId}", plan.Id);
            throw new InternalErrorException($"Failed to update subscription plan {plan.Id}", ex);
        }

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
        if (plan == null)
            throw new NotFoundException("SubscriptionPlan", id);

        try
        {
            _context.SubscriptionPlans.Remove(plan);
            await _context.SaveChangesAsync();
            _logger.LogInformation("SubscriptionPlan deleted: {PlanId}", plan.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete subscription plan {PlanId}", plan.Id);
            throw new InternalErrorException($"Failed to delete subscription plan {plan.Id}", ex);
        }
    }
}
