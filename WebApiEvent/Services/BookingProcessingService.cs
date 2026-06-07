using WebApiEvent.Models.Entity;
using WebApiEvent.Models.Enums;

namespace WebApiEvent.Services
{
    public class BookingProcessingService : BackgroundService
    {
        private readonly List<Booking> _bookings;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _processingDelay = TimeSpan.FromSeconds(2);

        public BookingProcessingService(List<Booking> bookings)
        {
            _bookings = bookings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<Booking> pendingBookings;
                lock (_bookings)
                {
                    pendingBookings = _bookings.Where(b => b.Status == BookingStatus.Pending).ToList();
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
