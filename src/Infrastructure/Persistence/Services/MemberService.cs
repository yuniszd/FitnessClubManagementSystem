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
    private readonly IQrCodeService _qrCodeService;

    public MemberService(
        IGenericRepository<Member> memberRepo,
        IGenericRepository<SubscriptionPlan> planRepo,
        IEmailService emailService,
        IQrCodeService qrCodeService)
    {
        _memberRepo = memberRepo;
        _planRepo = planRepo;
        _emailService = emailService;
        _qrCodeService = qrCodeService;

    }

    public async Task<Member> AddMemberAsync(CreateMemberDto dto)
    {
        var plan = await _planRepo.GetByIdAsync(dto.SubscriptionPlanId);
        if (plan == null) throw new Exception("Subscription plan not found");

        // unikal card/qr kod
        var cardNumber = Guid.NewGuid().ToString("N")[..8];

        var member = new Member
        {
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            CardNumber = cardNumber,
            JoinDate = DateTime.UtcNow
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

        // QR byte yarad
        var qrBytes = _qrCodeService.GenerateQrCode(cardNumber);
        // istəsən Member entity-də byte[] QrImage property açıb saxlayırsan

        //if (!string.IsNullOrWhiteSpace(member.Email))
        //{
        //    await _emailService.SendEmailAsync(
        //        member.Email,
        //        "Welcome to the Gym!",
        //        $"Salam {member.FullName}, abonementiniz {plan.Name} planı ilə aktivdir. Bitmə tarixi: {subscription.EndDate:dd/MM/yyyy}"
        //    );
        //}

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

    public async Task<bool> ValidateQrAsync(string qrCode)
    {
        // Member-i CardNumber ilə tap
        var member = await GetByCardAsync(qrCode);
        if (member == null) return false;

        // Aktiv və bitməmiş abonement varmı?
        var active = member.Subscriptions
            .Any(s => s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow);

        return active;
    }

    public async Task<Member?> GetByIdAsync(Guid id) => await _memberRepo.GetByIdAsync(id);

    public async Task<IEnumerable<Member>> GetAllAsync() => await _memberRepo.GetAllAsync();

    public async Task<Member?> GetByCardAsync(string cardNumber) =>
        (await _memberRepo.FindAsync(m => m.CardNumber == cardNumber)).FirstOrDefault();
}
