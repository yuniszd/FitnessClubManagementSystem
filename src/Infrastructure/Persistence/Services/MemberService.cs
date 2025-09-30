using System.Text.Json;
using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Application.Events;
using FCMS.Application.Extensions.Exceptions;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCMS.Persistence.Services;

public class MemberService : IMemberService
{
    private readonly IGenericRepository<Member> _memberRepo;
    private readonly IGenericRepository<SubscriptionPlan> _planRepo;
    private readonly IQrCodeService _qrCodeService;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly ILogger<MemberService> _logger;

    public MemberService(
        IGenericRepository<Member> memberRepo,
        IGenericRepository<SubscriptionPlan> planRepo,
        IQrCodeService qrCodeService,
        IRabbitMqPublisher rabbitMqPublisher,
        ILogger<MemberService> logger)
    {
        _memberRepo = memberRepo ?? throw new ArgumentNullException(nameof(memberRepo));
        _planRepo = planRepo ?? throw new ArgumentNullException(nameof(planRepo));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _rabbitMqPublisher = rabbitMqPublisher ?? throw new ArgumentNullException(nameof(rabbitMqPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Public Methods

    public async Task<Member> AddMemberAsync(CreateMemberDto dto)
    {
        if (dto == null)
            throw new ValidationException("dto", "CreateMemberDto cannot be null");

        var plan = await GetPlanOrThrowAsync(dto.SubscriptionPlanId);

        var member = CreateMember(dto);

        int allowedVisits = dto.AllowedVisits ?? plan.DefaultVisits;

        var subscription = CreateSubscription(member, plan, allowedVisits);
        member.Subscriptions.Add(subscription);

        await _memberRepo.AddAsync(member);
        await _memberRepo.SaveChangesAsync();

        await GenerateQrAndPublishEventAsync(member, subscription, plan);

        _logger.LogInformation("New member added: {MemberId}", member.Id);
        return member;
    }

    public async Task UpdateMemberAsync(UpdateMemberDto dto)
    {
        var member = await _memberRepo.GetByIdAsync(dto.Id)
                     ?? throw new NotFoundException("Member", dto.Id);

        member.FullName = dto.FullName;
        member.PhoneNumber = dto.PhoneNumber;
        member.Email = dto.Email;

        _memberRepo.Update(member);
        await _memberRepo.SaveChangesAsync();

        _logger.LogInformation("Member updated: {MemberId}", member.Id);
    }

    public async Task DeleteMemberAsync(Guid id)
    {
        var member = await GetByIdOrNullAsync(id)
                     ?? throw new NotFoundException("Member", id);

        _memberRepo.Remove(member);
        await _memberRepo.SaveChangesAsync();

        _logger.LogInformation("Member deleted: {MemberId}", id);
    }

    public async Task<bool> ValidateQrAsync(string qrCode)
    {
        if (string.IsNullOrWhiteSpace(qrCode))
            throw new ValidationException("qrCode", "QR code cannot be empty");

        var member = await GetByCardAsync(qrCode);
        if (member == null) return false;

        var isValid = member.Subscriptions.Any(s =>
            s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow);

        _logger.LogInformation("QR validation for {Card}: {Valid}", qrCode, isValid);
        return isValid;
    }

    public async Task<Member?> GetByIdAsync(Guid id) => await GetByIdOrNullAsync(id);
    public async Task<IEnumerable<Member>> GetAllAsync() => await _memberRepo.GetAllAsync();

    public async Task<Member?> GetByCardAsync(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new ValidationException("cardNumber", "Card number cannot be empty");

        return (await _memberRepo.FindAsync(m => m.CardNumber == cardNumber)).FirstOrDefault();
    }

    public async Task<(IEnumerable<Member> Members, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _memberRepo.GetQueryable();
        var totalCount = await query.CountAsync();

        var members = await query
            .OrderBy(m => m.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (members, totalCount);
    }

    public async Task<(IEnumerable<Member> Members, int TotalCount)> SearchPagedAsync(
        string? fullName, string? cardNumber, bool? isActive,
        int pageNumber, int pageSize)
    {
        var query = _memberRepo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(fullName))
            query = query.Where(m => m.FullName.Contains(fullName));

        if (!string.IsNullOrWhiteSpace(cardNumber))
            query = query.Where(m => m.CardNumber.Contains(cardNumber));

        if (isActive.HasValue)
            query = query.Where(m => m.Subscriptions.Any(s => s.EndDate >= DateTime.UtcNow) == isActive.Value);

        var totalCount = await query.CountAsync();

        var members = await query
            .OrderBy(m => m.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (members, totalCount);
    }

    #endregion

    #region Private Helpers

    private async Task<SubscriptionPlan> GetPlanOrThrowAsync(Guid planId)
    {
        var plan = await _planRepo.GetByIdAsync(planId)
                   ?? throw new NotFoundException("SubscriptionPlan", planId);

        _logger.LogInformation("Subscription plan found: {PlanId}", planId);
        return plan;
    }

    private Member CreateMember(CreateMemberDto dto)
    {
        return new Member
        {
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            CardNumber = Guid.NewGuid().ToString("N")[..8],
            JoinDate = DateTime.UtcNow
        };
    }

    private Subscription CreateSubscription(Member member, SubscriptionPlan plan, int allowedVisits)
    {
        return new Subscription
        {
            Member = member,
            SubscriptionPlan = plan,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(plan.DurationInMonths),
            AllowedVisits = allowedVisits
        };
    }

    private async Task GenerateQrAndPublishEventAsync(Member member, Subscription subscription, SubscriptionPlan plan)
    {
        try
        {
            var qrBytes = _qrCodeService.GenerateQrCode(member.CardNumber);

            if (string.IsNullOrWhiteSpace(member.Email))
            {
                _logger.LogWarning("Member email boşdur, event publish edilməyəcək: {MemberId}", member.Id);
                return;
            }

            var customerEvent = new CustomerRegisteredEvent
            {
                Email = member.Email,
                FullName = member.FullName,
                PlanName = plan.Name,
                SubscriptionEndDate = subscription.EndDate,
                QrCodeAttachment = qrBytes
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var eventJson = JsonSerializer.Serialize(customerEvent, options);
            await _rabbitMqPublisher.PublishAsync("customer_registered_queue", eventJson);

            _logger.LogInformation("CustomerRegisteredEvent published: {MemberId}", member.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate QR or publish event for member {MemberId}", member.Id);
        }
    }

    private async Task<Member?> GetByIdOrNullAsync(Guid id) => await _memberRepo.GetByIdAsync(id);

    #endregion
}
