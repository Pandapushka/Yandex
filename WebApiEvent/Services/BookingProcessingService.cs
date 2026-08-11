using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.DataAccess;
using WebApiEvent.Models.Enums;

namespace WebApiEvent.Services
{
    public class BookingProcessingService : BackgroundService
    {
        private readonly List<Booking> _bookings;
        private readonly List<Event> _events;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

        public BookingProcessingService(List<Booking> bookings)
        {
            _bookings = bookings;
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

                foreach (var booking in pendingBookings)
                {
                    await Task.Delay(_processingDelay, stoppingToken);

                    lock (_bookings)
                    {
                        var currentBooking = _bookings.FirstOrDefault(b => b.Id == booking.Id);
                        if (currentBooking != null && currentBooking.Status == BookingStatus.Pending)
                        {
                            currentBooking.Confirm();
                        }
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}