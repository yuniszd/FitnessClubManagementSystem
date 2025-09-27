using AutoFixture;
using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.DTOs.MemberDTOs;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using FCMS.Persistence.Services;
using Moq;
using System.Linq.Expressions;

namespace FCMS.Tests;

public class MemberServiceTests
{
    private readonly Mock<IGenericRepository<Member>> _memberRepoMock;
    private readonly Mock<IGenericRepository<SubscriptionPlan>> _planRepoMock;
    private readonly Mock<IQrCodeService> _qrCodeServiceMock;
    private readonly Mock<IRabbitMqPublisher> _rabbitMqPublisherMock;
    private readonly MemberService _service;
    private readonly Fixture _fixture;

    public MemberServiceTests()
    {
        _memberRepoMock = new Mock<IGenericRepository<Member>>();
        _planRepoMock = new Mock<IGenericRepository<SubscriptionPlan>>();
        _qrCodeServiceMock = new Mock<IQrCodeService>();
        _rabbitMqPublisherMock = new Mock<IRabbitMqPublisher>();
        _fixture = new Fixture();

        _service = new MemberService(
            _memberRepoMock.Object,
            _planRepoMock.Object,
            _qrCodeServiceMock.Object,
            _rabbitMqPublisherMock.Object
        );
    }

    #region AddMemberAsync Tests

    [Fact]
    public async Task AddMemberAsync_ShouldThrowInvalidOperationException_WhenSubscriptionPlanNotFound()
    {
        // Arrange
        var createMemberDto = _fixture.Build<CreateMemberDto>()
            .With(x => x.SubscriptionPlanId, Guid.NewGuid())
            .Create();

        _planRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((SubscriptionPlan?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddMemberAsync(createMemberDto));

        Assert.Equal("Subscription plan not found", exception.Message);

        _planRepoMock.Verify(x => x.GetByIdAsync(createMemberDto.SubscriptionPlanId), Times.Once);
        _memberRepoMock.Verify(x => x.AddAsync(It.IsAny<Member>()), Times.Never);
    }

    [Fact]
    public async Task AddMemberAsync_ShouldCreateMemberWithActiveSubscription_WhenValidDataProvided()
    {
        // Arrange
        var subscriptionPlan = _fixture.Build<SubscriptionPlan>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.DurationInMonths, 3)
            .With(x => x.Price, 100.00m)
            .Create();

        var createMemberDto = _fixture.Build<CreateMemberDto>()
            .With(x => x.SubscriptionPlanId, subscriptionPlan.Id)
            .With(x => x.AllowedVisits, 12)
            .Create();

        _planRepoMock.Setup(x => x.GetByIdAsync(subscriptionPlan.Id))
            .ReturnsAsync(subscriptionPlan);

        Member capturedMember = null;
        _memberRepoMock.Setup(x => x.AddAsync(It.IsAny<Member>()))
            .Callback<Member>(m => capturedMember = m)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddMemberAsync(createMemberDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createMemberDto.FullName, result.FullName);
        Assert.Equal(createMemberDto.Email, result.Email);
        Assert.Equal(createMemberDto.PhoneNumber, result.PhoneNumber);

        // CardNumber yoxlayırıq
        Assert.NotNull(result.CardNumber);
        Assert.StartsWith("FCMS", result.CardNumber);

        // JoinDate avtomatik set olunmalıdır
        Assert.True(result.JoinDate <= DateTime.UtcNow);

        // Verify subscription
        Assert.Single(result.Subscriptions);
        var subscription = result.Subscriptions.First();
        Assert.Equal(subscriptionPlan.Id, subscription.SubscriptionPlanId);
        Assert.Equal(createMemberDto.AllowedVisits, subscription.AllowedVisits);
        Assert.Equal(0, subscription.UsedVisits);
        Assert.True(subscription.IsActive);
        Assert.True(subscription.StartDate <= DateTime.UtcNow);
        Assert.True(subscription.EndDate > DateTime.UtcNow);

        // Verify repository interactions
        _memberRepoMock.Verify(x => x.AddAsync(It.IsAny<Member>()), Times.Once);
        _memberRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        // Verify messaging - QR code service çağırılmır, çünki Member-də QrCode property-si yoxdur
        _qrCodeServiceMock.Verify(x => x.GenerateQrCode(It.IsAny<string>()), Times.Never);

        // RabbitMQ çağırılır
        _rabbitMqPublisherMock.Verify(x =>
            x.PublishAsync("customer_registered_queue", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddMemberAsync_ShouldHandleNullOptionalFields_WhenEmailAndPhoneNotProvided()
    {
        // Arrange
        var subscriptionPlan = _fixture.Create<SubscriptionPlan>();
        var createMemberDto = _fixture.Build<CreateMemberDto>()
            .With(x => x.SubscriptionPlanId, subscriptionPlan.Id)
            .With(x => x.Email, (string?)null)
            .With(x => x.PhoneNumber, (string?)null)
            .Create();

        _planRepoMock.Setup(x => x.GetByIdAsync(subscriptionPlan.Id))
            .ReturnsAsync(subscriptionPlan);

        // Act
        var result = await _service.AddMemberAsync(createMemberDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createMemberDto.FullName, result.FullName);
        Assert.Null(result.Email);
        Assert.Null(result.PhoneNumber);
        Assert.NotNull(result.CardNumber);
    }

    [Fact]
    public async Task AddMemberAsync_ShouldGenerateUniqueCardNumber_ForEachMember()
    {
        // Arrange
        var subscriptionPlan = _fixture.Create<SubscriptionPlan>();
        var createMemberDto = _fixture.Create<CreateMemberDto>();

        _planRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(subscriptionPlan);

        List<string> generatedCardNumbers = new();
        _memberRepoMock.Setup(x => x.AddAsync(It.IsAny<Member>()))
            .Callback<Member>(m => generatedCardNumbers.Add(m.CardNumber))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddMemberAsync(createMemberDto);
        await _service.AddMemberAsync(createMemberDto);

        // Assert
        Assert.Equal(2, generatedCardNumbers.Count);
        Assert.NotEqual(generatedCardNumbers[0], generatedCardNumbers[1]);
        Assert.All(generatedCardNumbers, cn => Assert.StartsWith("FCMS", cn));
    }

    #endregion

    #region UpdateMemberAsync Tests

    [Fact]
    public async Task UpdateMemberAsync_ShouldUpdateMemberAndSaveChanges_WhenMemberIsValid()
    {
        // Arrange
        var member = _fixture.Build<Member>()
            .With(x => x.Id, Guid.NewGuid())
            .Without(x => x.Subscriptions)
            .Create();

        // Act
        await _service.UpdateMemberAsync(member);

        // Assert
        _memberRepoMock.Verify(x => x.Update(member), Times.Once);
        _memberRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateMemberAsync_ShouldThrowArgumentNullException_WhenMemberIsNull()
    {
        // Arrange
        Member nullMember = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateMemberAsync(nullMember));
    }

    #endregion

    #region DeleteMemberAsync Tests

    [Fact]
    public async Task DeleteMemberAsync_ShouldRemoveMemberAndSaveChanges_WhenMemberExists()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var existingMember = _fixture.Build<Member>()
            .With(x => x.Id, memberId)
            .Create();

        _memberRepoMock.Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(existingMember);

        // Act
        await _service.DeleteMemberAsync(memberId);

        // Assert
        _memberRepoMock.Verify(x => x.GetByIdAsync(memberId), Times.Once);
        _memberRepoMock.Verify(x => x.Remove(existingMember), Times.Once);
        _memberRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteMemberAsync_ShouldDoNothing_WhenMemberDoesNotExist()
    {
        // Arrange
        var nonExistentMemberId = Guid.NewGuid();

        _memberRepoMock.Setup(x => x.GetByIdAsync(nonExistentMemberId))
            .ReturnsAsync((Member?)null);

        // Act
        await _service.DeleteMemberAsync(nonExistentMemberId);

        // Assert
        _memberRepoMock.Verify(x => x.GetByIdAsync(nonExistentMemberId), Times.Once);
        _memberRepoMock.Verify(x => x.Remove(It.IsAny<Member>()), Times.Never);
        _memberRepoMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteMemberAsync_ShouldThrowArgumentException_WhenMemberIdIsEmpty()
    {
        // Arrange
        var emptyMemberId = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.DeleteMemberAsync(emptyMemberId));
    }

    #endregion

    #region ValidateQrAsync Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ValidateQrAsync_ShouldReturnFalse_WhenQrCodeDataIsInvalid(string invalidQrCode)
    {
        // Act
        var result = await _service.ValidateQrAsync(invalidQrCode);

        // Assert
        Assert.False(result);
        _memberRepoMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task ValidateQrAsync_ShouldReturnFalse_WhenMemberNotFound()
    {
        // Arrange
        var qrCodeData = "FCMS20231010120000";

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member>());

        // Act
        var result = await _service.ValidateQrAsync(qrCodeData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateQrAsync_ShouldReturnFalse_WhenMemberHasNoSubscriptions()
    {
        // Arrange
        var qrCodeData = "FCMS20231010120000";
        var member = _fixture.Build<Member>()
            .With(x => x.CardNumber, qrCodeData)
            .Without(x => x.Subscriptions)
            .Create();

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member> { member });

        // Act
        var result = await _service.ValidateQrAsync(qrCodeData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateQrAsync_ShouldReturnFalse_WhenAllSubscriptionsExpired()
    {
        // Arrange
        var qrCodeData = "FCMS20231010120000";
        var member = _fixture.Build<Member>()
            .With(x => x.CardNumber, qrCodeData)
            .With(x => x.Subscriptions, new List<Subscription>
            {
                new Subscription
                {
                    StartDate = DateTime.UtcNow.AddMonths(-3),
                    EndDate = DateTime.UtcNow.AddMonths(-1),
                    AllowedVisits = 10,
                    UsedVisits = 5
                },
                new Subscription
                {
                    StartDate = DateTime.UtcNow.AddMonths(-6),
                    EndDate = DateTime.UtcNow.AddMonths(-4),
                    AllowedVisits = 20,
                    UsedVisits = 10
                }
            })
            .Create();

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member> { member });

        // Act
        var result = await _service.ValidateQrAsync(qrCodeData);

        // Assert
        Assert.False(result);
        Assert.All(member.Subscriptions, s => Assert.False(s.IsActive));
    }

    [Fact]
    public async Task ValidateQrAsync_ShouldReturnTrue_WhenActiveSubscriptionExists()
    {
        // Arrange
        var qrCodeData = "FCMS20231010120000";
        var member = _fixture.Build<Member>()
            .With(x => x.CardNumber, qrCodeData)
            .With(x => x.Subscriptions, new List<Subscription>
            {
                new Subscription
                {
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(20),
                    AllowedVisits = 10,
                    UsedVisits = 5
                }
            })
            .Create();

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member> { member });

        // Act
        var result = await _service.ValidateQrAsync(qrCodeData);

        // Assert
        Assert.True(result);
        Assert.True(member.Subscriptions.First().IsActive);
    }

    [Fact]
    public async Task ValidateQrAsync_ShouldReturnTrue_WhenUnlimitedVisitsSubscriptionExists()
    {
        // Arrange
        var qrCodeData = "FCMS20231010120000";
        var member = _fixture.Build<Member>()
            .With(x => x.CardNumber, qrCodeData)
            .With(x => x.Subscriptions, new List<Subscription>
            {
                new Subscription
                {
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(20),
                    AllowedVisits = null, // Limitsiz ziyarət
                    UsedVisits = 1000
                }
            })
            .Create();

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member> { member });

        // Act
        var result = await _service.ValidateQrAsync(qrCodeData);

        // Assert
        Assert.True(result);
        Assert.True(member.Subscriptions.First().IsActive);
    }

    [Fact]
    public async Task ValidateQrAsync_ShouldReturnFalse_WhenSubscriptionVisitsExhausted()
    {
        // Arrange
        var qrCodeData = "FCMS20231010120000";
        var member = _fixture.Build<Member>()
            .With(x => x.CardNumber, qrCodeData)
            .With(x => x.Subscriptions, new List<Subscription>
            {
                new Subscription
                {
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(20),
                    AllowedVisits = 10,
                    UsedVisits = 10 // Bütün ziyarətlər istifadə edilib
                }
            })
            .Create();

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member> { member });

        // Act
        var result = await _service.ValidateQrAsync(qrCodeData);

        // Assert
        Assert.False(result);
        Assert.False(member.Subscriptions.First().IsActive);
    }

    #endregion

    #region Get Methods Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMember_WhenMemberExists()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var expectedMember = _fixture.Build<Member>()
            .With(x => x.Id, memberId)
            .Create();

        _memberRepoMock.Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(expectedMember);

        // Act
        var result = await _service.GetByIdAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.Id);
        Assert.Equal(expectedMember.FullName, result.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMemberDoesNotExist()
    {
        // Arrange
        var nonExistentMemberId = Guid.NewGuid();

        _memberRepoMock.Setup(x => x.GetByIdAsync(nonExistentMemberId))
            .ReturnsAsync((Member?)null);

        // Act
        var result = await _service.GetByIdAsync(nonExistentMemberId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMembers_WhenMembersExist()
    {
        // Arrange
        var expectedMembers = _fixture.CreateMany<Member>(5).ToList();

        _memberRepoMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(expectedMembers);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        Assert.IsAssignableFrom<IEnumerable<Member>>(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyCollection_WhenNoMembersExist()
    {
        // Arrange
        _memberRepoMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Member>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByCardAsync_ShouldReturnMember_WhenCardNumberExists()
    {
        // Arrange
        var cardNumber = "FCMS20231010120000";
        var expectedMember = _fixture.Build<Member>()
            .With(x => x.CardNumber, cardNumber)
            .Create();

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member> { expectedMember });

        // Act
        var result = await _service.GetByCardAsync(cardNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cardNumber, result.CardNumber);
        Assert.Equal(expectedMember.Id, result.Id);
    }

    [Fact]
    public async Task GetByCardAsync_ShouldReturnNull_WhenCardNumberDoesNotExist()
    {
        // Arrange
        var nonExistentCardNumber = "NONEXISTENT123";

        _memberRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
            .ReturnsAsync(new List<Member>());

        // Act
        var result = await _service.GetByCardAsync(nonExistentCardNumber);

        // Assert
        Assert.Null(result);
    }

    #endregion
}