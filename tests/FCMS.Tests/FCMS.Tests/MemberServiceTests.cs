using AutoFixture;
using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using FCMS.Persistence.Services;
using FCMS.Application.Extensions.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace FCMS.Tests;

public class MemberServiceTests
{
    private readonly Mock<IGenericRepository<Member>> _memberRepoMock;
    private readonly Mock<IGenericRepository<SubscriptionPlan>> _planRepoMock;
    private readonly Mock<IQrCodeService> _qrCodeServiceMock;
    private readonly Mock<IRabbitMqPublisher> _rabbitMqPublisherMock;
    private readonly Mock<ILogger<MemberService>> _loggerMock;
    private readonly MemberService _service;
    private readonly Fixture _fixture;

    public MemberServiceTests()
    {
        _memberRepoMock = new Mock<IGenericRepository<Member>>();
        _planRepoMock = new Mock<IGenericRepository<SubscriptionPlan>>();
        _qrCodeServiceMock = new Mock<IQrCodeService>();
        _rabbitMqPublisherMock = new Mock<IRabbitMqPublisher>();
        _loggerMock = new Mock<ILogger<MemberService>>();
        _fixture = new Fixture();

        _service = new MemberService(
            _memberRepoMock.Object,
            _planRepoMock.Object,
            _qrCodeServiceMock.Object,
            _rabbitMqPublisherMock.Object,
            _loggerMock.Object
        );
    }

    #region AddMemberAsync Tests

    [Fact]
    public async Task AddMemberAsync_ShouldThrowNotFoundException_WhenPlanNotFound()
    {
        var dto = _fixture.Build<CreateMemberDto>()
            .With(x => x.SubscriptionPlanId, Guid.NewGuid())
            .Create();

        _planRepoMock.Setup(x => x.GetByIdAsync(dto.SubscriptionPlanId))
            .ReturnsAsync((SubscriptionPlan?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.AddMemberAsync(dto));

        Assert.Contains("SubscriptionPlan", ex.Message);
        _planRepoMock.Verify(x => x.GetByIdAsync(dto.SubscriptionPlanId), Times.Once);
        _memberRepoMock.Verify(x => x.AddAsync(It.IsAny<Member>()), Times.Never);
    }

    [Fact]
    public async Task AddMemberAsync_ShouldCreateMemberSuccessfully_WhenValidInput()
    {
        var plan = _fixture.Build<SubscriptionPlan>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.DurationInMonths, 3)
            .Create();

        var dto = _fixture.Build<CreateMemberDto>()
            .With(x => x.SubscriptionPlanId, plan.Id)
            .With(x => x.AllowedVisits, 12)
            .Create();

        _planRepoMock.Setup(x => x.GetByIdAsync(plan.Id)).ReturnsAsync(plan);

        Member capturedMember = null!;
        _memberRepoMock.Setup(x => x.AddAsync(It.IsAny<Member>()))
            .Callback<Member>(m => capturedMember = m)
            .Returns(Task.CompletedTask);

        _qrCodeServiceMock.Setup(x => x.GenerateQrCode(It.IsAny<string>()))
            .Returns(Array.Empty<byte>());

        _rabbitMqPublisherMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _service.AddMemberAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.FullName, result.FullName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.PhoneNumber, result.PhoneNumber);
        Assert.NotNull(result.CardNumber);
        Assert.Single(result.Subscriptions);

        var sub = result.Subscriptions.First();
        Assert.Equal(12, sub.AllowedVisits);
        Assert.True(sub.IsActive);

        _memberRepoMock.Verify(x => x.AddAsync(It.IsAny<Member>()), Times.Once);
        _memberRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _rabbitMqPublisherMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region UpdateMemberAsync Tests

    [Fact]
    public async Task UpdateMemberAsync_ShouldCallUpdateAndSave_WhenMemberExists()
    {
        var member = _fixture.Create<Member>();
        var dto = new UpdateMemberDto(
            Id: member.Id,
            FullName: member.FullName,
            PhoneNumber: member.PhoneNumber,
            Email: member.Email
        );

        _memberRepoMock.Setup(x => x.GetByIdAsync(dto.Id)).ReturnsAsync(member);

        await _service.UpdateMemberAsync(dto);

        _memberRepoMock.Verify(x => x.Update(It.Is<Member>(m =>
            m.Id == member.Id &&
            m.FullName == dto.FullName &&
            m.PhoneNumber == dto.PhoneNumber &&
            m.Email == dto.Email)), Times.Once);

        _memberRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateMemberAsync_ShouldThrowNotFoundException_WhenMemberDoesNotExist()
    {
        var dto = new UpdateMemberDto(
            Id: Guid.NewGuid(),
            FullName: "Test Name",
            PhoneNumber: "123456",
            Email: "test@example.com"
        );

        _memberRepoMock.Setup(x => x.GetByIdAsync(dto.Id)).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateMemberAsync(dto));
    }

    #endregion

    #region DeleteMemberAsync Tests

    [Fact]
    public async Task DeleteMemberAsync_ShouldRemoveMember_WhenExists()
    {
        var member = _fixture.Create<Member>();
        _memberRepoMock.Setup(x => x.GetByIdAsync(member.Id)).ReturnsAsync(member);

        await _service.DeleteMemberAsync(member.Id);

        _memberRepoMock.Verify(x => x.Remove(member), Times.Once);
        _memberRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteMemberAsync_ShouldThrowNotFoundException_WhenDoesNotExist()
    {
        var id = Guid.NewGuid();
        _memberRepoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteMemberAsync(id));
    }

    #endregion

    #region ValidateQrAsync Tests

    [Fact]
    public async Task ValidateQrAsync_ShouldReturnFalse_WhenMemberNotFound()
    {
        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Member>());

        var result = await _service.ValidateQrAsync("INVALID");
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateQrAsync_ShouldReturnTrue_WhenMemberHasActiveSubscription()
    {
        var member = _fixture.Build<Member>()
            .With(x => x.CardNumber, "CARD123")
            .With(x => x.Subscriptions, new List<Subscription>
            {
                new Subscription
                {
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(10),
                    AllowedVisits = 10,
                    UsedVisits = 5
                }
            })
            .Create();

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member> { member });

        var result = await _service.ValidateQrAsync("CARD123");
        Assert.True(result);
    }

    #endregion

    #region Get Methods Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMember_WhenExists()
    {
        var member = _fixture.Create<Member>();
        _memberRepoMock.Setup(x => x.GetByIdAsync(member.Id)).ReturnsAsync(member);

        var result = await _service.GetByIdAsync(member.Id);
        Assert.Equal(member.Id, result!.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMembers()
    {
        var members = _fixture.CreateMany<Member>(5).ToList();
        _memberRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(members);

        var result = await _service.GetAllAsync();
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task GetByCardAsync_ShouldReturnMember_WhenExists()
    {
        var member = _fixture.Create<Member>();
        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member> { member });

        var result = await _service.GetByCardAsync(member.CardNumber);
        Assert.Equal(member.CardNumber, result!.CardNumber);
    }

    #endregion
}
