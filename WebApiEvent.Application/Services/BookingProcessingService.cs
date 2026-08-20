using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Domain.Enums;

namespace WebApiEvent.Application.Services
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
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    pendingBookingIds = await bookingRepository.GetPendingBookingIdsAsync(stoppingToken);
                }

                foreach (var bookingId in pendingBookingIds)
                {
                    await Task.Delay(_processingDelay, stoppingToken);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        var booking = await bookingRepository.GetByIdAsync(bookingId, stoppingToken);
                        if (booking != null && booking.Status == BookingStatus.Pending)
                        {
                            booking.Confirm();
                            await bookingRepository.SaveChangesAsync(stoppingToken);
                        }
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
