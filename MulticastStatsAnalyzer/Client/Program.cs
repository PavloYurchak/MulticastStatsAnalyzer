using System.Threading.Channels;
using Client.Abstractions;
using Client.Config;
using Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Models;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = ClientConfigLoader.Load("ClientConfig.xml");
        services.AddSingleton(config);

        //var channel = Channel.CreateUnbounded<QuoteMessage>();

        var channel = Channel.CreateBounded<QuoteMessage>(
        new BoundedChannelOptions(capacity: 1000)  // підбери під навантаження
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropOldest // або Wait
        });
        services.AddSingleton(channel);
        services.AddSingleton(channel.Writer);
        services.AddSingleton(channel.Reader);

        services.AddHostedService<MulticastReceiver>();

        services.AddSingleton<StatisticsService>();
        services.AddSingleton<IStatisticsService>(sp => sp.GetRequiredService<StatisticsService>());
        services.AddHostedService(sp => sp.GetRequiredService<StatisticsService>());

        services.AddHostedService<ConsoleCommandListener>();

    })
    .Build()
    .Run();