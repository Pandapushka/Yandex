using FluentAssertions;
using Moq;
using Events.Application;
using Events.Application.DTOs.Event;
using Events.Application.Interfaces;
using Events.Application.Options;
using Events.Application.Services;
using Events.Domain.Entities;

namespace Events.Tests;

public class EventServiceCachingTests
{
    private readonly Mock<IEventRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly EventService _service;
    private readonly Guid _eventId = Guid.NewGuid();

    public EventServiceCachingTests()
    {
        _repositoryMock = new Mock<IEventRepository>();
        _cacheMock = new Mock<ICacheService>();
        _service = new EventService(_repositoryMock.Object, _cacheMock.Object, new CacheOptions());
    }

    [Fact]
    public async Task GetById_CacheHit_DoesNotCallRepository()
    {
        var cachedDto = new EventDtoResponse(
            _eventId, "Заголовок", "Описание",
            DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 40, 100);

        _cacheMock
            .Setup(c => c.GetAsync<EventDtoResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        var result = await _service.GetByIdAsync(_eventId);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Заголовок");
        _repositoryMock.Verify(
            r => r.GetActiveByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetById_CacheMiss_LoadsFromRepositoryAndCaches()
    {
        var entity = Event.Create("Заголовок", "Описание", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 100);

        _cacheMock
            .Setup(c => c.GetAsync<EventDtoResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventDtoResponse?)null);
        _repositoryMock
            .Setup(r => r.GetActiveByIdAsync(_eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(_eventId);

        result.Should().NotBeNull();
        _repositoryMock.Verify(
            r => r.GetActiveByIdAsync(_eventId, It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<EventDtoResponse>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_AfterSave_InvalidatesEventCacheKey()
    {
        var entity = Event.Create("Заголовок", "Описание", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 100);
        _repositoryMock
            .Setup(r => r.GetActiveByIdAsync(_eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _service.UpdateAsync(_eventId, new UpdateEventDtoRequest { Title = "Новый заголовок" });

        _cacheMock.Verify(
            c => c.RemoveAsync(CacheKeys.Event(_eventId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_AfterSave_InvalidatesEventCacheKey()
    {
        var entity = Event.Create("Заголовок", "Описание", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 100);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _service.DeleteAsync(_eventId);

        _cacheMock.Verify(
            c => c.RemoveAsync(CacheKeys.Event(_eventId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DecreaseAvailableSeats_AfterSave_InvalidatesEventCacheKey()
    {
        var entity = Event.Create("Заголовок", "Описание", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 100);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(_eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _service.DecreaseAvailableSeatsAsync(_eventId, 2);

        _cacheMock.Verify(
            c => c.RemoveAsync(CacheKeys.Event(_eventId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTopEvents_CacheHit_DoesNotCallRepository()
    {
        var cached = new List<EventDtoResponse>
        {
            new(Guid.NewGuid(), "Событие", "Описание", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10, 100)
        };

        _cacheMock
            .Setup(c => c.GetAsync<List<EventDtoResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await _service.GetTopEventsAsync();

        result.Should().HaveCount(1);
        _repositoryMock.Verify(
            r => r.GetTopBySoldPercentageAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTopEvents_CacheMiss_LoadsFromRepositoryAndCaches()
    {
        _cacheMock
            .Setup(c => c.GetAsync<List<EventDtoResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<EventDtoResponse>?)null);
        _repositoryMock
            .Setup(r => r.GetTopBySoldPercentageAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event>());

        var result = await _service.GetTopEventsAsync();

        result.Should().BeEmpty();
        _repositoryMock.Verify(
            r => r.GetTopBySoldPercentageAsync(10, It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.SetAsync(
                CacheKeys.Top10,
                It.IsAny<List<EventDtoResponse>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
