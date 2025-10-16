using ConsumerConsoleApp;
using ConsumerConsoleApp.Config;
using ConsumerConsoleApp.Configurations;
using ConsumerConsoleApp.Consumers;
using ConsumerConsoleApp.Helper;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// 測試推送訊息
// await MessagePusher.PushTestMessageAsync(10);



try
{
    var builder = Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .UseSerilog((hostingContext, services, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        })
        .ConfigureServices((context, services) =>
        {
            // 註冊您的 Worker 服務
            // Worker 繼承自 BackgroundService，它將作為一個長時間運行的託管服務
            services.AddHostedService<Worker>();

            services.AddHttpClient();

            var consumerConfigs = new List<(Type EventType, string Url, string Method, string QueueName)>();

            var forwardingSection = context.Configuration.GetSection("ApiForwarding");
            var baseUrls = forwardingSection.GetSection("BaseUrl").Get<Dictionary<string, string>>();

            foreach (var child in forwardingSection.GetChildren())
            {
                var eventName = child.Key;
                if (eventName == "BaseUrl") continue; // 跳過 BaseUrl

                var config = new ApiForwardingConfig();
                child.Bind(config);

                var eventType = Type.GetType($"Contract.Events.{eventName}");
                if (eventType == null) continue;


                // 自動組合完整 URL
                if (!string.IsNullOrEmpty(config.BaseUrlKey) &&
                    baseUrls != null &&
                    baseUrls.TryGetValue(config.BaseUrlKey, out var baseUrl))
                {
                    config.Url = baseUrl.TrimEnd('/') + config.Path;
                }


                var queueName = QueueNameHelper.GetQueueName(eventName);

                consumerConfigs.Add((eventType, config.Url, config.Method, queueName));

                services.Configure<ApiForwardingConfig>(eventName, options =>
                {
                    options.BaseUrlKey = config.BaseUrlKey;
                    options.Path = config.Path;
                    options.Url = config.Url;
                    options.Method = config.Method;
                });

            }



            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                // 1. 註冊所有事件對應的 Consumer 類型
                // 對每個事件型別建立 ApiForwardingConsumer<T>，並註冊到 MassTransit
                foreach (var config in consumerConfigs)
                {
                    var consumerType = typeof(ApiForwardingConsumer<>).MakeGenericType(config.EventType);
                    x.AddConsumer(consumerType);
                }


                var rabbitMqConfig = context.Configuration
                                    .GetSection("RabbitMq")
                                    .Get<RabbitMqConfig>();

                var rabbitMqUri = new Uri($"rabbitmq://{rabbitMqConfig.Host}:{rabbitMqConfig.Port}/");

                // 2. 配置 RabbitMQ 傳輸與接收端點
                x.UsingRabbitMq((context, cfg) =>
                {
                    // 設定 RabbitMQ 主機參數（主機位置、使用者名稱、密碼）
                    cfg.Host(rabbitMqUri, rabbitMqConfig.VirtualHost, h =>
                    {
                        h.Username(rabbitMqConfig.UserName);
                        h.Password(rabbitMqConfig.Password);
                    });

                    cfg.UseRawJsonSerializer();

                    foreach (var config in consumerConfigs)
                    {
                        var consumerType = typeof(ApiForwardingConsumer<>).MakeGenericType(config.EventType);

                        cfg.ReceiveEndpoint(config.QueueName, e =>
                        {
                            e.PrefetchCount = 3; // 一次最多取x個訊息進來
                            e.ConcurrentMessageLimit = 1;  // 每次只處理x筆 

                            // 延長 Timeout，預設大約 30 秒，這裡拉到 10 分鐘
                            e.UseTimeout(t => t.Timeout = TimeSpan.FromMinutes(10));

                            // 加入錯誤重試
                            //e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                            // 將 Consumer 與此 ReceiveEndpoint 綁定
                            e.ConfigureConsumer(context, consumerType);
                        });
                    }
                });
            });

        })
        .Build();

    await builder.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}




