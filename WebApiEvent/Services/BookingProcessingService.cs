using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.DataAccess;
using WebApiEvent.Models.Enums;

namespace WebApiEvent.Services
{
    public class BookingProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _processingDelay = TimeSpan.FromSeconds(2);

        public BookingProcessingService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<Guid> pendingBookingIds;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    pendingBookingIds = db.Bookings
                        .Where(b => b.Status == BookingStatus.Pending)
                        .Select(b => b.Id)
                        .ToList();
                }

                foreach (var bookingId in pendingBookingIds)
                {
                    await Task.Delay(_processingDelay, stoppingToken);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
                        if (booking != null && booking.Status == BookingStatus.Pending)
                        {
                            booking.Confirm();
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}