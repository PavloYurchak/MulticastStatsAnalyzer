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

        var channel = Channel.CreateUnbounded<QuoteMessage>();
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