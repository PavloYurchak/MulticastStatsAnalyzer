using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure

{
    public abstract class AbstractBackgroundService<T>(ILogger<T> logger,
        string serviceName,
        bool repeat) : BackgroundService
    {
        protected abstract Task DoWorkAsync(CancellationToken stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"Starting {serviceName}...");

            do
            {
                try
                {
                    await DoWorkAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation($"{serviceName} has been cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{serviceName} encountered an error. Retrying immediately.");
                }
            }
            while (repeat && !stoppingToken.IsCancellationRequested);
            logger.LogInformation($"{serviceName} has stopped.");
        }
    }
}
