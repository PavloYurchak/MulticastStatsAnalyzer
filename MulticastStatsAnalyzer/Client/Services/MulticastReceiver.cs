using Client.Config;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure;
using Shared.Models;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Channels;

namespace Client.Services
{
    internal class MulticastReceiver(ClientConfig clientConfig,
        ILogger<MulticastReceiver> logger,
        ChannelWriter<QuoteMessage> channelWriter)
        : AbstractBackgroundService<MulticastReceiver>(logger, nameof(MulticastReceiver), repeat: true)
    {
        protected override async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            using var udpClient = new UdpClient(AddressFamily.InterNetwork);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, clientConfig.Port));
            udpClient.JoinMulticastGroup(IPAddress.Parse(clientConfig.MulticastAddress));

            udpClient.Client.ReceiveBufferSize = clientConfig.BufferSize;
            udpClient.Client.ReceiveTimeout = clientConfig.ReceiveTimeoutMs;

            LogInformation($"Listening for multicast messages on {clientConfig.MulticastAddress}:{clientConfig.Port}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync(stoppingToken);
                    var message = JsonSerializer.Deserialize<QuoteMessage>(result.Buffer);

                    if(!channelWriter.TryWrite(message))
                    {
                        await channelWriter.WriteAsync(message, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    LogInformation("Multicast receive cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error receiving UDP multicast message.");
                }
            }
        }
    }
}
