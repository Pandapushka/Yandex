using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.CustomExceptions;
using WebApiEvent.DataAccess;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;
using WebApiEvent.Services;

namespace WebApiEvent.Tests;

public class EventServiceTests
{
    private readonly IServiceProvider _serviceProvider;

    public EventServiceTests()
    {
        _testData = new List<Event>
        {
            Event.Create("Конференция A", "Описание A", new DateTime(2026, 1, 10, 9, 0, 0), new DateTime(2026, 1, 10, 18, 0, 0), 100),
            Event.Create("Митап B", "Описание B", new DateTime(2026, 2, 15, 18, 0, 0), new DateTime(2026, 2, 15, 21, 0, 0), 50),
            Event.Create("Воркшоп C", "Описание C", new DateTime(2026, 3, 5, 10, 0, 0), new DateTime(2026, 3, 5, 17, 0, 0), 30),
        };
        _service = new EventService(_testData);
    }

    [Fact]
    public async Task Create_ValidEvent_ReturnsIdAndAddsToList()
    {
        var request = new EventDtoRequest("Новое событие", "Описание", new DateTime(2026, 5, 1, 10, 0, 0), new DateTime(2026, 5, 1, 18, 0, 0));

        var id = await service.CreateAsync(request);
        var all = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });

        id.Should().NotBeEmpty();
        all.Items.Should().HaveCount(4);
        all.Items.Should().Contain(e => e.Id == id && e.Title == "Новое событие");
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyActiveEventsWithPaginationAndFilters()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var all = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });
        var eventToDeactivate = all.Items.First();
        await service.SoftDeleteAsync(eventToDeactivate.Id);

        var result = await service.GetAllAsync(new EventRequestDto { Page = 1, PageSize = 10 });

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().NotContain(e => e.Id == eventToDeactivate.Id);
    }

    [Fact]
    public async Task GetById_ExistingActiveEvent_ReturnsEventDto()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var all = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });
        var existingEvent = all.Items.First();

        var result = await service.GetByIdAsync(existingEvent.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(existingEvent.Id);
        result.Title.Should().Be(existingEvent.Title);
    }

    [Fact]
    public async Task Update_ValidUpdate_ModifiesEvent()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var all = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });
        var existing = all.Items.First();
        var updateRequest = new UpdateEventDtoRequest
        {
            Title = "Обновлённый заголовок",
            Description = "Новое описание"
        };

        await service.UpdateAsync(existing.Id, updateRequest);
        var updated = await service.GetByIdAsync(existing.Id);

        updated!.Title.Should().Be("Обновлённый заголовок");
        updated.Description.Should().Be("Новое описание");
    }

    [Fact]
    public async Task Delete_ExistingEvent_RemovesItCompletely()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var all = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });
        var existing = all.Items.First();
        var id = existing.Id;

        await service.DeleteAsync(id);

        Func<Task> act = async () => await service.GetByIdAsync(id);
        await act.Should().ThrowAsync<NotFoundException>();
        var remaining = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });
        remaining.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task SoftDelete_ActiveEvent_DeactivatesIt()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var all = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });
        var target = all.Items.First();

        await service.SoftDeleteAsync(target.Id);

        Func<Task> act = async () => await service.GetByIdAsync(target.Id);
        await act.Should().ThrowAsync<NotFoundException>();
        var allActive = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });
        allActive.Items.Should().NotContain(e => e.Id == target.Id);
    }

    [Fact]
    public async Task FilterByTitle_ReturnsCaseInsensitivePartialMatches()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var request = new EventRequestDto { Title = "конферен" };

        var result = await service.GetAllAsync(request);

        result.Items.Should().ContainSingle(e => e.Title == "Конференция A");
    }

    [Fact]
    public async Task FilterByDateRange_ReturnsEventsWithinRange()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var request = new EventRequestDto
        {
            From = new DateTime(2026, 2, 1),
            To = new DateTime(2026, 2, 28)
        };

        var result = await service.GetAllAsync(request);

        result.Items.Should().ContainSingle(e => e.Title == "Митап B");
    }

    [Fact]
    public async Task Pagination_ReturnsCorrectPageAndTotalPages()
    {
        _service.Create(new EventDtoRequest("Event4", "", new DateTime(2026, 4, 1, 10, 0, 0), new DateTime(2026, 4, 1, 18, 0, 0)));
        _service.Create(new EventDtoRequest("Event5", "", new DateTime(2026, 5, 1, 10, 0, 0), new DateTime(2026, 5, 1, 18, 0, 0)));

        var result1 = await service.GetAllAsync(new EventRequestDto { Page = 1, PageSize = 2 });
        result1.Items.Should().HaveCount(2);
        result1.TotalCount.Should().Be(5);
        result1.TotalPages.Should().Be(3);
        result1.Page.Should().Be(1);

        var result2 = await service.GetAllAsync(new EventRequestDto { Page = 2, PageSize = 2 });
        result2.Items.Should().HaveCount(2);
        result2.Page.Should().Be(2);
    }

    [Fact]
    public void CombinedFilters_ApplyAllTogether()
    {
        _service.Create(new EventDtoRequest("Special Conference", "", new DateTime(2026, 2, 20, 10, 0, 0), new DateTime(2026, 2, 20, 18, 0, 0)));

        var request = new EventRequestDto
        {
            Title = "conference",
            From = new DateTime(2026, 2, 10),
            To = new DateTime(2026, 2, 28),
            Page = 1,
            PageSize = 10
        };
        var result = _service.GetAll(request);
        result.Items.Should().ContainSingle(e => e.Title.Contains("Special", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetById_NonExistingId_ThrowsNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        Func<Task> act = async () => await service.GetByIdAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*не найдено*");
    }

    [Fact]
    public async Task Update_NonExistingId_ThrowsNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var updateRequest = new UpdateEventDtoRequest { Title = "New" };
        Func<Task> act = async () => await service.UpdateAsync(Guid.NewGuid(), updateRequest);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_InvalidDates_ThrowsCustomValidationException()
    {
        var request = new EventDtoRequest(
            "Test",
            "Desc",
            new DateTime(2026, 5, 10, 10, 0, 0),
            new DateTime(2026, 5, 9, 18, 0, 0)
        );

        Func<Task> act = async () => await service.CreateAsync(request);
        await act.Should().ThrowAsync<CustomValidationException>().WithMessage("*позже даты начала*");
    }

    [Fact]
    public async Task Update_InvalidDates_ThrowsCustomValidationException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var all = await service.GetAllAsync(new EventRequestDto { PageSize = 100 });
        var existing = all.Items.First();
        var updateRequest = new UpdateEventDtoRequest
        {
            StartAt = new DateTime(2026, 5, 10, 10, 0, 0),
            EndAt = new DateTime(2026, 5, 9, 18, 0, 0)
        };

        Func<Task> act = async () => await service.UpdateAsync(existing.Id, updateRequest);
        await act.Should().ThrowAsync<CustomValidationException>();
    }

    [Fact]
    public async Task Pagination_NegativePage_DefaultsToOne()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var result = await service.GetAllAsync(new EventRequestDto { Page = -5, PageSize = 10 });
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task Pagination_PageSizeTooLarge_CapsAt50()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var result = await service.GetAllAsync(new EventRequestDto { PageSize = 200 });
        result.PageSize.Should().Be(50);
    }
}