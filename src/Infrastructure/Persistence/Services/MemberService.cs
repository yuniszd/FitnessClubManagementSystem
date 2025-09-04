using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Domain.Entities;

namespace FCMS.Persistence.Services;

public class MemberService : IMemberService
{
    private readonly IGenericRepository<Member> _memberRepo;
    private readonly IGenericRepository<SubscriptionPlan> _planRepo;
    private readonly IEmailService _emailService;

    public MemberService(
        IGenericRepository<Member> memberRepo,
        IGenericRepository<SubscriptionPlan> planRepo,
        IEmailService emailService)
    {
        _memberRepo = memberRepo;
        _planRepo = planRepo;
        _emailService = emailService;
    }

    public async Task<Member> AddMemberAsync(CreateMemberDto dto)
    {
        var plan = await _planRepo.GetByIdAsync(dto.SubscriptionPlanId);
        if (plan == null) throw new Exception("Subscription plan not found");

        var member = new Member
        {
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email
        };

        var subscription = new Subscription
        {
            Member = member,
            SubscriptionPlan = plan,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(plan.DurationInMonths),
            AllowedVisits = dto.AllowedVisits
        };

        member.Subscriptions.Add(subscription);

        await _memberRepo.AddAsync(member);
        await _memberRepo.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(member.Email))
        {
            await _emailService.SendEmailAsync(
                member.Email,
                "Welcome to the Gym!",
                $"Salam {member.FullName}, abonementiniz {plan.Name} planı ilə aktivdir. Bitmə tarixi: {subscription.EndDate:dd/MM/yyyy}"
            );
        }

        return member;
    }

    public async Task UpdateMemberAsync(Member member)
    {
        _memberRepo.Update(member);
        await _memberRepo.SaveChangesAsync();
    }

    public async Task DeleteMemberAsync(Guid id)
    {
        var member = await _memberRepo.GetByIdAsync(id);
        if (member != null)
        {
            _memberRepo.Remove(member);
            await _memberRepo.SaveChangesAsync();
        }
    }

    public async Task<Member?> GetByIdAsync(Guid id) => await _memberRepo.GetByIdAsync(id);

    public async Task<IEnumerable<Member>> GetAllAsync() => await _memberRepo.GetAllAsync();

    public async Task<Member?> GetByCardAsync(string cardNumber) =>
        (await _memberRepo.FindAsync(m => m.CardNumber == cardNumber)).FirstOrDefault();
}
