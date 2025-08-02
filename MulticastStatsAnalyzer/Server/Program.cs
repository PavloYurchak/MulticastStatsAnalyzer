using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Config;
using Server.Service;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(ConfigLoader.Load("ServerConfig.xml"));
        services.AddHostedService<UdpBroadcastService>();
    })
    .Build()
    .Run();