using System.Text.Json;
using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Application.Events;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;

namespace FCMS.Persistence.Services
{
    public class MemberService : IMemberService
    {
        private readonly IGenericRepository<Member> _memberRepo;
        private readonly IGenericRepository<SubscriptionPlan> _planRepo;
        private readonly IQrCodeService _qrCodeService;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public MemberService(
            IGenericRepository<Member> memberRepo,
            IGenericRepository<SubscriptionPlan> planRepo,
            IQrCodeService qrCodeService,
            IRabbitMqPublisher rabbitMqPublisher)
        {
            _memberRepo = memberRepo;
            _planRepo = planRepo;
            _qrCodeService = qrCodeService;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public async Task<Member> AddMemberAsync(CreateMemberDto dto)
        {
            var plan = await _planRepo.GetByIdAsync(dto.SubscriptionPlanId);
            if (plan == null)
                throw new InvalidOperationException("Subscription plan not found");

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

            // ✅ QR kod yarat
            var qrBytes = _qrCodeService.GenerateQrCode(cardNumber);

            // ✅ Event göndər
            if (!string.IsNullOrWhiteSpace(member.Email))
            {
                var customerEvent = new CustomerRegisteredEvent
                {
                    Email = member.Email!,
                    FullName = member.FullName,
                    PlanName = plan.Name,
                    SubscriptionEndDate = subscription.EndDate,
                    QrCodeAttachment = qrBytes
                };

                var eventJson = JsonSerializer.Serialize(customerEvent);
                await _rabbitMqPublisher.PublishAsync("customer_registered_queue", eventJson);
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

        public async Task<bool> ValidateQrAsync(string qrCode)
        {
            var member = await GetByCardAsync(qrCode);
            if (member == null) return false;

            return member.Subscriptions.Any(s => s.StartDate <= DateTime.UtcNow &&
                                                 s.EndDate >= DateTime.UtcNow);
        }

        public async Task<Member?> GetByIdAsync(Guid id) =>
            await _memberRepo.GetByIdAsync(id);

        public async Task<IEnumerable<Member>> GetAllAsync() =>
            await _memberRepo.GetAllAsync();

        public async Task<Member?> GetByCardAsync(string cardNumber) =>
            (await _memberRepo.FindAsync(m => m.CardNumber == cardNumber)).FirstOrDefault();
    }
}
