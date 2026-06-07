using WebApiEvent.Models.Entity;
using WebApiEvent.Models.Enums;

namespace WebApiEvent.Services
{
    public class BookingProcessingService : BackgroundService
    {
        private readonly List<Booking> _bookings;
        private readonly List<Event> _events;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

        public BookingProcessingService(List<Booking> bookings, List<Event> events)
        {
            _bookings = bookings;
            _events = events;
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

                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
                await Task.WhenAll(tasks);

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                await _processingSemaphore.WaitAsync(stoppingToken);
                try
                {
                    var eventEntity = _events.FirstOrDefault(e => e.Id == booking.EventId);
                    if (eventEntity == null || !eventEntity.IsActive)
                    {
                        booking.Reject();
                        return;
                    }

                    booking.Confirm();
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception)
            {
                await _processingSemaphore.WaitAsync(stoppingToken);
                try
                {
                    var eventEntity = _events.FirstOrDefault(e => e.Id == booking.EventId);
                    if (eventEntity != null && eventEntity.IsActive)
                    {
                        eventEntity.ReleaseSeats(1);
                    }
                    booking.Reject();
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }
        }
    }
}