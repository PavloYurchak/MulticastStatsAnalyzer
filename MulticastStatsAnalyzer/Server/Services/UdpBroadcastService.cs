using Microsoft.Extensions.Logging;
using Server.Config;
using Server.Infrastructure;
using Server.Model;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Service
{
    internal class UdpBroadcastService(ILogger<UdpBroadcastService> logger,
        ServerConfig serverConfig) 
        : AbstractBackgroundService<UdpBroadcastService>(logger, nameof(UdpBroadcastService), repeat: true)
    {
        private readonly ServerConfig _serverConfig = serverConfig;
        private readonly Random _random = new();
        private long _id = 1;
        protected override async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            using var udpClient = new UdpClient(_serverConfig.Port);
            var endPoint = new IPEndPoint(IPAddress.Parse(_serverConfig.MulticastAddress), _serverConfig.Port);
            
            double value = Math.Round(
                _random.NextDouble() * (_serverConfig.MaxValue - _serverConfig.MinValue) + _serverConfig.MinValue,
                _serverConfig.Decimals
                );

            if(_id == long.MaxValue)
                _id = 1;

            var quote = new QuoteMessage
            {
                Id = _id++,
                Value = value,
            };

            string json = System.Text.Json.JsonSerializer.Serialize(quote);
            byte[] data = Encoding.UTF8.GetBytes(json);

            await udpClient.SendAsync(data, endPoint, cancellationToken: stoppingToken);
        }
    }
}
