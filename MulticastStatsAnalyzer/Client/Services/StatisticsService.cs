using Client.Abstractions;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure;
using Shared.Models;
using System.Threading.Channels;

namespace Client.Services
{
    internal class StatisticsService(ChannelReader<QuoteMessage> channelReader,
        ILogger<StatisticsService> logger)
        : AbstractBackgroundService<StatisticsService>(logger, nameof(StatisticsService), repeat: true), IStatisticsService
    {
        private long _lastId = 0;
        private long _received = 0;
        private long _lost = 0;
        private readonly List<double> _values = new();
        protected override async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            var message = await channelReader.ReadAsync(stoppingToken);

            Interlocked.Increment(ref _received);

            if (_lastId != 0 && message.Id != _lastId - 1)
            {
                long missed = message.Id - _lastId - 1;
                if(missed > 0)
                    Interlocked.Add(ref _lost, missed);
            }

            _lastId = message.Id;

            lock (_values)
            {
                _values.Add(message.Value);
            }
        }

        public void PrintStatistics()
        {
            lock (_values)
            {
                if(_values.Count == 0)
                {
                    LogInformation("No values received yet.");
                    return;
                }

                var values = _values.ToArray();
                double mean = values.Average();

                double stddev = Math.Sqrt(values
                    .Select(v => Math.Pow(v - mean, 2))
                    .Sum() / values.Length);

                double median = values.OrderBy(v => v)
                    .ElementAt(values.Length / 2);

                double mode = values.GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                Console.WriteLine($"""
                            Statistics:
                            - Received: {_received}
                            - Lost: {_lost}
                            - Mean: {mean:F2}
                            - Standard Deviation: {stddev:F2}
                            - Median: {median:F2}
                            - Mode: {mode:F2}
                            """);


            }
        }
    }
}
