using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using WebApiEvent.DataAccess;
using WebApiEvent.DataAccess.Repositories;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;

namespace EventApi.IntegrationTests
{
    public class EventRepositoryTests : IClassFixture<PostgreSqlFixture>
    {
        private readonly PostgreSqlFixture _fixture;

        public EventRepositoryTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
        }

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_fixture.ConnectionString));
            services.AddScoped<IEventRepository, EventRepository>();
            return services.BuildServiceProvider();
        }

        private async Task ResetDatabaseAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        [Fact]
        public async Task AddAsync_ValidEvent_PersistsToDatabase()
        {
            var sp = CreateServiceProvider();
            await ResetDatabaseAsync(sp);

            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var ev = Event.Create("Test", "Desc",
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Utc));
            await repo.AddAsync(ev);

            var found = await repo.GetActiveByIdAsync(ev.Id);
            found.Should().NotBeNull();
            found!.Title.Should().Be("Test");
        }

        [Fact]
        public async Task GetAllAsync_WithFilters_ReturnsFilteredResults()
        {
            var sp = CreateServiceProvider();
            await ResetDatabaseAsync(sp);

            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var now = DateTime.UtcNow;
            var e1 = Event.Create("Alpha", "D1",
                DateTime.SpecifyKind(now.AddDays(1), DateTimeKind.Utc),
                DateTime.SpecifyKind(now.AddDays(1).AddHours(2), DateTimeKind.Utc));
            var e2 = Event.Create("Beta", "D2",
                DateTime.SpecifyKind(now.AddDays(2), DateTimeKind.Utc),
                DateTime.SpecifyKind(now.AddDays(2).AddHours(2), DateTimeKind.Utc));
            await repo.AddAsync(e1);
            await repo.AddAsync(e2);

            var request = new EventRequestDto { Title = "Al" };
            var result = await repo.GetAllAsync(request);

            result.Items.Should().ContainSingle(e => e.Title == "Alpha");
        }

        [Fact]
        public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
        {
            var sp = CreateServiceProvider();
            await ResetDatabaseAsync(sp);

            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var now = DateTime.UtcNow;
            for (int i = 1; i <= 5; i++)
            {
                var ev = Event.Create($"Event{i}", "",
                    DateTime.SpecifyKind(now.AddDays(i), DateTimeKind.Utc),
                    DateTime.SpecifyKind(now.AddDays(i).AddHours(2), DateTimeKind.Utc));
                await repo.AddAsync(ev);
            }

            var page1 = await repo.GetAllAsync(new EventRequestDto { Page = 1, PageSize = 2 });
            page1.Items.Should().HaveCount(2);
            page1.TotalCount.Should().Be(5);
            page1.TotalPages.Should().Be(3);
        }

        [Fact]
        public async Task DeleteAsync_ExistingEvent_RemovesIt()
        {
            var sp = CreateServiceProvider();
            await ResetDatabaseAsync(sp);

            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var ev = Event.Create("ToDelete", "",
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Utc));
            await repo.AddAsync(ev);

            await repo.DeleteAsync(ev);

            var found = await repo.GetByIdAsync(ev.Id);
            found.Should().BeNull();
        }

        [Fact]
        public async Task SoftDelete_DeactivatesEvent()
        {
            var sp = CreateServiceProvider();
            await ResetDatabaseAsync(sp);

            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var ev = Event.Create("Soft", "",
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Utc));
            await repo.AddAsync(ev);

            ev.Deactivate();
            await repo.SaveChangesAsync();

            var found = await repo.GetActiveByIdAsync(ev.Id);
            found.Should().BeNull();
        }
    }
}