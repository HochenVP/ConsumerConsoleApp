using ConsumerConsoleApp;
using ConsumerConsoleApp.Extensions;
using ConsumerConsoleApp.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;


try
{
    var builder = Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .UseSerilog((hostingContext, services, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        })
        .ConfigureServices((context , services) =>
        {
            // 註冊您的 Worker 服務
            // Worker 繼承自 BackgroundService，它將作為一個長時間運行的託管服務
            services.AddHostedService<Worker>();

            services.AddHttpClient();
            services.AddScoped<ApiForwardingHelper>();

            //註冊 ApiForwarding 的各個 Consumer 配置到 DI 容器中。
            services.RegisterApiForwardingConfigs(context);

            // 註冊 MassTransit 與所有 Consumers
            services.AddMassTransitWithConsumers(context);

        })
        .Build();

    await builder.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}




