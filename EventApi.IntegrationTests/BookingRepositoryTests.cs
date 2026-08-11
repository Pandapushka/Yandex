using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.DataAccess;
using WebApiEvent.DataAccess.Repositories;
using WebApiEvent.Models.Entity;
using WebApiEvent.Models.Enums;

namespace EventApi.IntegrationTests
{
    public class BookingRepositoryTests : IClassFixture<PostgreSqlFixture>
    {
        private readonly PostgreSqlFixture _fixture;

        public BookingRepositoryTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
        }

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_fixture.ConnectionString));
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            return services.BuildServiceProvider();
        }

        private async Task ResetDatabaseAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        private async Task<Event> CreateEventAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            var ev = Event.Create("Test Event", "",
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Utc));
            await eventRepo.AddAsync(ev);
            return ev;
        }

        [Fact]
        public async Task AddAsync_ValidBooking_PersistsToDatabase()
        {
            var sp = CreateServiceProvider();
            await ResetDatabaseAsync(sp);

            var ev = await CreateEventAsync(sp);

            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var booking = Booking.CreatePending(ev.Id);
            await repo.AddAsync(booking);

            var found = await repo.GetByIdAsync(booking.Id);
            found.Should().NotBeNull();
            found!.Status.Should().Be(BookingStatus.Pending);
        }

        [Fact]
        public async Task GetPendingBookingIdsAsync_ReturnsOnlyPending()
        {
            var sp = CreateServiceProvider();
            await ResetDatabaseAsync(sp);

            var ev = await CreateEventAsync(sp);

            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            var b1 = Booking.CreatePending(ev.Id);
            await repo.AddAsync(b1);

            var b2 = Booking.CreatePending(ev.Id);
            b2.Confirm();
            await repo.AddAsync(b2);

            var pendingIds = await repo.GetPendingBookingIdsAsync();
            pendingIds.Should().Contain(b1.Id);
            pendingIds.Should().NotContain(b2.Id);
        }

        [Fact]
        public async Task SaveChangesAsync_UpdatesBookingStatus()
        {
            var sp = CreateServiceProvider();
            await ResetDatabaseAsync(sp);

            var ev = await CreateEventAsync(sp);

            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var booking = Booking.CreatePending(ev.Id);
            await repo.AddAsync(booking);

            booking.Confirm();
            await repo.SaveChangesAsync();

            var updated = await repo.GetByIdAsync(booking.Id);
            updated!.Status.Should().Be(BookingStatus.Confirmed);
        }
    }
}