namespace backend.services
{
    public class TimedHostedService(IServiceProvider serviceProvider, ILogger<TimedHostedService> logger)
        : IHostedService, IDisposable
    {
        private PeriodicTimer _timer = null!;

        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Timed Hosted Service running.");
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            Task.Run(async () =>
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWork(stoppingToken);
                }
            }, stoppingToken);

            return Task.CompletedTask;
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();
            var updateService = scope.ServiceProvider.GetRequiredService<UpdateService>();

            logger.LogInformation("Updating Contest Information at {Time}", DateTime.UtcNow);
            await updateService.CheckContests();
            await updateService.UpdateLeaderboard();
            await updateService.UpdateContests();
            await updateService.GetCompetitors();
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Timed Hosted Service is stopping.");
            _timer?.Dispose();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}