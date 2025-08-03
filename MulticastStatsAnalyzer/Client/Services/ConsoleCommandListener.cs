using Client.Abstractions;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure;

namespace Client.Services
{
    internal class ConsoleCommandListener(IStatisticsService statisticsService,
        ILogger<ConsoleCommandListener> logger)
        : AbstractBackgroundService<ConsoleCommandListener>(logger, nameof(ConsoleCommandListener), repeat: true)
    {
        protected override async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if ( key.Key == ConsoleKey.Enter)
                {
                    statisticsService.PrintStatistics();
                }
            }

            await Task.Delay(100, stoppingToken);
        }
    }
}
