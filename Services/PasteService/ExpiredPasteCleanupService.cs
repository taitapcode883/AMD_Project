using Microsoft.EntityFrameworkCore;
using PasteService.Data;

namespace PasteService
{
    public class ExpiredPasteCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

        public ExpiredPasteCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    await context.Pastes
                        .Where(p => p.ExpiresAt != null && p.ExpiresAt < DateTime.UtcNow)
                        .ExecuteDeleteAsync(stoppingToken);
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}