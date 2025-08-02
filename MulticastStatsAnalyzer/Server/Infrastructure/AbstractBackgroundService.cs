using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Server.Infrastructure

{
    public abstract class AbstractBackgroundService<T>(ILogger<T> logger,
        string serviceName,
        bool repeat) : BackgroundService
    {
        protected abstract Task DoWorkAsync(CancellationToken stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"Starting {serviceName}...");
            try
            {
                do
                {
                    await DoWorkAsync(stoppingToken);
                } while (repeat && !stoppingToken.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation($"{serviceName} has been cancelled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{serviceName} encountered an error.");
            }
            finally
            {
                logger.LogInformation($"{serviceName} has stopped.");
            }
        }
    }
}
