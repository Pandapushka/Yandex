using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.DataAccess.Repositories;
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
                    var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    pendingBookingIds = await bookingRepo.GetPendingBookingIdsAsync();
                }

                foreach (var bookingId in pendingBookingIds)
                {
                    await Task.Delay(_processingDelay, stoppingToken);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        var booking = await bookingRepo.GetByIdAsync(bookingId);
                        if (booking != null && booking.Status == BookingStatus.Pending)
                        {
                            booking.Confirm();
                            await bookingRepo.SaveChangesAsync();
                        }
                    }
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}