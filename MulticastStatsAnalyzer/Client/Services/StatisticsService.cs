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

        private double _average = 0;

        private double _mean = 0;
        private double _m2 = 0;
        private double _stddev = 0;

        private PriorityQueue<double, double> _lower = new();
        private PriorityQueue<double, double> _upper = new();
        private readonly Queue<double> _window = new();
        private readonly Dictionary<double, int> _toRemove = new();
        private const int MaxWindowSize = 20;
        private double _median = 0;

        private Dictionary<double, int> _frequency = new();
        private double _mode = 0;
        private int _maxFrequency = 0;
        private const int MaxModeEntries = 5000;
        

        protected override async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            var message = await channelReader.ReadAsync(stoppingToken);

            Interlocked.Increment(ref _received);

            if (_lastId != 0 && message.Id != _lastId - 1)
            {
                long missed = message.Id - _lastId - 1;
                if (missed > 0)
                    Interlocked.Add(ref _lost, missed);
            }

            _lastId = message.Id;
            double value = message.Value;

            UpdateAverage(value);
            UpdateStdDev(value);
            UpdateMedian(value);
            UpdateMode(value);
        }

        public void PrintStatistics()
        {
            if (_received == 0)
            {
                LogInformation("No values received yet.");
                return;
            }

            LogInformation($"""
            Statistics:
            - Received: {_received}
            - Lost: {_lost}
            - Average: {_average:F2}
            - StdDev: {_stddev:F2}
            - Median: {_median:F2}
            - Mode: {_mode:F2}
            """);
        }

        private void UpdateAverage(double newValue)
        {
            _average += (newValue - _average) / _received;
        }

        private void UpdateStdDev(double newValue)
        {
            double delta = newValue - _mean;
            _mean += delta / _received;
            double delta2 = newValue - _mean;
            _m2 += delta * delta2;

            if (_received > 1)
                _stddev = Math.Sqrt(_m2 / _received);
        }

        private void UpdateMedian(double newValue)
        {
            _window.Enqueue(newValue);

            if (_lower.Count == 0 || newValue <= GetLowerTop())
                _lower.Enqueue(-newValue, -newValue);   
            else
                _upper.Enqueue(newValue, newValue);     

            PruneHeap(_lower, isLower: true);
            PruneHeap(_upper, isLower: false);

            BalanceHeaps();

            if (_window.Count > MaxWindowSize)
            {
                double old = _window.Dequeue();
                _toRemove[old] = _toRemove.TryGetValue(old, out var c) ? c + 1 : 1;

                PruneHeap(_lower, isLower: true);
                PruneHeap(_upper, isLower: false);
                BalanceHeaps();
            }

            if (_lower.Count == _upper.Count)
                _median = (GetLowerTop() + GetUpperTop()) / 2.0;
            else
                _median = GetLowerTop();
        }

        private void UpdateMode(double newValue)
        {
            double rounded = Math.Round(newValue, 2);

            if (_frequency.TryGetValue(rounded, out int count))
            {
                _frequency[rounded] = count + 1;
            }
            else
            {
                if (_frequency.Count >= MaxModeEntries)
                {
                    var leastUsed = _frequency.OrderBy(kv => kv.Value).First();
                    _frequency.Remove(leastUsed.Key);
                }
                _frequency[rounded] = 1;
            }

            if (_frequency[rounded] > _maxFrequency)
            {
                _maxFrequency = _frequency[rounded];
                _mode = rounded;
            }
        }

        private void BalanceHeaps()
        {
            while (_lower.Count > _upper.Count + 1)
            {
                double v = -_lower.Dequeue();
                _upper.Enqueue(v, v);
                PruneHeap(_lower, isLower: true);
            }

            while (_upper.Count > _lower.Count)
            {
                double v = _upper.Dequeue();
                _lower.Enqueue(-v, -v);
                PruneHeap(_upper, isLower: false);
            }
        }

        private void PruneHeap(PriorityQueue<double, double> heap, bool isLower)
        {
            while (heap.Count > 0)
            {
                double top = isLower ? -heap.Peek() : heap.Peek();
                if (_toRemove.TryGetValue(top, out int cnt) && cnt > 0)
                {
                    heap.Dequeue();
                    if (--cnt == 0) _toRemove.Remove(top);
                    else _toRemove[top] = cnt;
                }
                else
                {
                    break;
                }
            }
        }

        private double GetLowerTop() => -_lower.Peek();
        private double GetUpperTop() => _upper.Peek();
    }
}
