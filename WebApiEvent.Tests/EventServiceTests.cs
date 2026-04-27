using FluentAssertions;
using WebApiEvent.CustomExceptions;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;
using WebApiEvent.Services;

namespace WebApiEvent.Tests;

public class EventServiceTests
{
    private readonly List<Event> _testData;
    private readonly EventService _service;

    public EventServiceTests()
    {
        _testData = new List<Event>
        {
            Event.Create("Конференция A", "Описание A", new DateTime(2026, 1, 10, 9, 0, 0), new DateTime(2026, 1, 10, 18, 0, 0)),
            Event.Create("Митап B", "Описание B", new DateTime(2026, 2, 15, 18, 0, 0), new DateTime(2026, 2, 15, 21, 0, 0)),
            Event.Create("Воркшоп C", "Описание C", new DateTime(2026, 3, 5, 10, 0, 0), new DateTime(2026, 3, 5, 17, 0, 0)),
        };
        _service = new EventService(_testData);
    }

    [Fact]
    public void Create_ValidEvent_ReturnsIdAndAddsToList()
    {
        var request = new EventDtoRequest("Новое событие", "Описание", new DateTime(2026, 5, 1, 10, 0, 0), new DateTime(2026, 5, 1, 18, 0, 0));

        var id = _service.Create(request);
        var all = _service.GetAll(new EventRequestDto { PageSize = 100 });

        id.Should().NotBeEmpty();
        all.Items.Should().HaveCount(4);
        all.Items.Should().Contain(e => e.Id == id && e.Title == "Новое событие");
    }

    [Fact]
    public void GetAll_ReturnsOnlyActiveEventsWithPaginationAndFilters()
    {
        var eventToDeactivate = _testData.First();
        _service.SoftDelete(eventToDeactivate.Id);

        var request = new EventRequestDto { Page = 1, PageSize = 10 };

        var result = _service.GetAll(request);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().NotContain(e => e.Id == eventToDeactivate.Id);
    }

    [Fact]
    public void GetById_ExistingActiveEvent_ReturnsEventDto()
    {
        var existingEvent = _testData.First();

        var result = _service.GetById(existingEvent.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(existingEvent.Id);
        result.Title.Should().Be(existingEvent.Title);
    }

    [Fact]
    public void Update_ValidUpdate_ModifiesEvent()
    {
        var existing = _testData.First();
        var updateRequest = new UpdateEventDtoRequest
        {
            Title = "Обновлённый заголовок",
            Description = "Новое описание"
        };

        _service.Update(existing.Id, updateRequest);
        var updated = _service.GetById(existing.Id);

        updated.Title.Should().Be("Обновлённый заголовок");
        updated.Description.Should().Be("Новое описание");
    }

    [Fact]
    public void Delete_ExistingEvent_RemovesItCompletely()
    {
        var existing = _testData.First();
        var id = existing.Id;

        _service.Delete(id);

        Action act = () => _service.GetById(id);
        act.Should().Throw<NotFoundException>();
        _service.GetAll(new EventRequestDto { PageSize = 100 }).Items.Should().HaveCount(2);
    }

    [Fact]
    public void SoftDelete_ActiveEvent_DeactivatesIt()
    {
        var target = _testData.First();

        _service.SoftDelete(target.Id);

        Action act = () => _service.GetById(target.Id);
        act.Should().Throw<NotFoundException>();
        var allActive = _service.GetAll(new EventRequestDto { PageSize = 100 });
        allActive.Items.Should().NotContain(e => e.Id == target.Id);
    }

    [Fact]
    public void FilterByTitle_ReturnsCaseInsensitivePartialMatches()
    {
        var request = new EventRequestDto { Title = "конферен" };

        var result = _service.GetAll(request);

        result.Items.Should().ContainSingle(e => e.Title == "Конференция A");
    }

    [Fact]
    public void FilterByDateRange_ReturnsEventsWithinRange()
    {
        var request = new EventRequestDto
        {
            From = new DateTime(2026, 2, 1),
            To = new DateTime(2026, 2, 28)
        };

        var result = _service.GetAll(request);

        result.Items.Should().ContainSingle(e => e.Title == "Митап B");
        result.Items.Should().NotContain(e => e.Title == "Конференция A");
        result.Items.Should().NotContain(e => e.Title == "Воркшоп C");
    }

    [Fact]
    public void Pagination_ReturnsCorrectPageAndTotalPages()
    {
        _service.Create(new EventDtoRequest("Event4", "", new DateTime(2026, 4, 1, 10, 0, 0), new DateTime(2026, 4, 1, 18, 0, 0)));
        _service.Create(new EventDtoRequest("Event5", "", new DateTime(2026, 5, 1, 10, 0, 0), new DateTime(2026, 5, 1, 18, 0, 0)));

        var requestPage1 = new EventRequestDto { Page = 1, PageSize = 2 };
        var result1 = _service.GetAll(requestPage1);
        result1.Items.Should().HaveCount(2);
        result1.TotalCount.Should().Be(5);
        result1.TotalPages.Should().Be(3);
        result1.Page.Should().Be(1);

        var requestPage2 = new EventRequestDto { Page = 2, PageSize = 2 };
        var result2 = _service.GetAll(requestPage2);
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
        Action act = () => _service.GetById(Guid.NewGuid());
        act.Should().Throw<NotFoundException>().WithMessage("*не найдено*");
    }

    [Fact]
    public void Update_NonExistingId_ThrowsNotFoundException()
    {
        var updateRequest = new UpdateEventDtoRequest { Title = "New" };
        Action act = () => _service.Update(Guid.NewGuid(), updateRequest);
        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Create_InvalidDates_ThrowsCustomValidationException()
    {
        var request = new EventDtoRequest(
            "Test",
            "Desc",
            new DateTime(2026, 5, 10, 10, 0, 0),
            new DateTime(2026, 5, 9, 18, 0, 0)
        );

        Action act = () => _service.Create(request);
        act.Should().Throw<CustomValidationException>().WithMessage("*позже даты начала*");
    }

    [Fact]
    public void Update_InvalidDates_ThrowsCustomValidationException()
    {
        var existing = _testData.First();
        var updateRequest = new UpdateEventDtoRequest
        {
            StartAt = new DateTime(2026, 5, 10, 10, 0, 0),
            EndAt = new DateTime(2026, 5, 9, 18, 0, 0)
        };

        Action act = () => _service.Update(existing.Id, updateRequest);
        act.Should().Throw<CustomValidationException>();
    }

    [Fact]
    public void FilterWithEmptyTitle_ReturnsAllActiveEvents()
    {
        var request = new EventRequestDto { Title = "" };
        var result = _service.GetAll(request);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public void Pagination_NegativePage_DefaultsToOne()
    {
        var request = new EventRequestDto { Page = -5, PageSize = 10 };
        var result = _service.GetAll(request);
        result.Page.Should().Be(1);
    }

    [Fact]
    public void Pagination_PageSizeTooLarge_CapsAt50()
    {
        var request = new EventRequestDto { PageSize = 200 };
        var result = _service.GetAll(request);
        result.PageSize.Should().Be(50);
    }
}