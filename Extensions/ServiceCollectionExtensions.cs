using ConsumerConsoleApp.Config;
using ConsumerConsoleApp.Configurations;
using ConsumerConsoleApp.Consumers;
using ConsumerConsoleApp.Queue;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsumerConsoleApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 將 "ApiForwarding" 配置節點中的各個 Consumer 設定註冊到 <see cref="IServiceCollection"/> 中。
        /// 每個 Consumer 會對應一個 <see cref="ApiForwardingConfig"/> 的命名配置，方便在程式中透過 <see cref="IOptionsSnapshot{T}"/> 取得。
        /// </summary>
        /// <param name="services">要註冊配置的 <see cref="IServiceCollection"/> 實例。</param>
        /// <param name="context">包含應用程式設定的 <see cref="HostBuilderContext"/>，用來讀取 IConfiguration。</param>
        /// <returns>回傳同一個 <see cref="IServiceCollection"/>，以支援方法鏈式呼叫。</returns>
        /// <remarks>
        /// 這個方法會：
        /// 1. 讀取 "ApiForwarding" 配置節點下的所有 Consumer 子節點（排除 "BaseUrl"）。
        /// 2. 將 BaseUrl 與 Path 自動組合成完整 URL。
        /// 3. 使用 <see cref="services.Configure{TOptions}(IServiceCollection, Action{TOptions})"/> 註冊每個 Consumer 的 <see cref="ApiForwardingConfig"/>。
        /// </remarks>
        public static IServiceCollection RegisterApiForwardingConfigs(this IServiceCollection services, HostBuilderContext context)
        {

            var forwardingSection = context.Configuration.GetSection("ApiForwarding");
            var baseUrls = forwardingSection.GetSection("BaseUrl").Get<Dictionary<string, string>>();

            foreach (var child in forwardingSection.GetChildren())
            {
                var consumerClassName = child.Key;
                if (consumerClassName == "BaseUrl") continue; // 跳過 BaseUrl

                var config = new ApiForwardingConfig();
                child.Bind(config);


                // 自動組合完整 URL
                if (!string.IsNullOrEmpty(config.BaseUrlKey) &&
                    baseUrls != null &&
                    baseUrls.TryGetValue(config.BaseUrlKey, out var baseUrl))
                {
                    config.Url = baseUrl.TrimEnd('/') + config.Path;
                }

                services.Configure<ApiForwardingConfig>(consumerClassName, options =>
                {
                    options.BaseUrlKey = config.BaseUrlKey;
                    options.Path = config.Path;
                    options.Url = config.Url;
                    options.Method = config.Method;
                });

            }
            return services;

        }


        /// <summary>
        /// 註冊 MassTransit 與 RabbitMQ，並綁定 Consumers 與對應 Queue。
        /// </summary>
        /// <param name="services">IServiceCollection DI 容器</param>
        /// <param name="configuration">HostBuilderContext.Configuration</param>
        public static IServiceCollection AddMassTransitWithConsumers(
            this IServiceCollection services , HostBuilderContext context)
        {
            // 讀取 RabbitMQ 配置
            var rabbitMqConfig = context.Configuration.GetSection("RabbitMq").Get<RabbitMqConfig>();
            var rabbitMqUri = new Uri($"rabbitmq://{rabbitMqConfig.Host}:{rabbitMqConfig.Port}/");

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                // 註冊所有 Consumer
                x.RegisterAllConsumers();

                // 配置 RabbitMQ 傳輸
                x.UsingRabbitMq((context, cfg) =>
                {
                    // 設定 RabbitMQ 主機參數
                    cfg.Host(rabbitMqUri, rabbitMqConfig.VirtualHost, h =>
                    {
                        h.Username(rabbitMqConfig.UserName);
                        h.Password(rabbitMqConfig.Password);
                    });

                    // 使用原始 JSON Serializer
                    cfg.UseRawJsonSerializer();

                    //設定所有 RabbitMQ Queue 與 Consumer 綁定
                    cfg.ConfigureQueues(context);
                });
            });

            return services;
        }


        /// <summary>
        /// 註冊所有 MassTransit Consumer 類別
        /// </summary>
        /// <param name="x">MassTransit 配置物件</param>
        public static void RegisterAllConsumers(this IBusRegistrationConfigurator x)
        {
            x.AddConsumer<AllPhotoOriginalAnalysisConsumer>();
            // 如果有新的 Consumer，只要在這裡新增即可
        }

        /// <summary>
        /// 設定所有 RabbitMQ Queue 與 Consumer 綁定
        /// </summary>
        /// <param name="cfg">RabbitMQ 配置物件</param>
        /// <param name="context">BusRegistrationContext</param>
        public static void ConfigureQueues(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
        {
            //3. QueueKey 與 Consumer 綁定
            cfg.ReceiveEndpoint(QueueKey.AllPhotoOriginalAnalysis.ToQueueName(), e =>
            {
                e.PrefetchCount = 3; // 一次最多取x個訊息進來
                e.ConcurrentMessageLimit = 1;  // 每次只處理x筆 
                e.ConfigureConsumer<AllPhotoOriginalAnalysisConsumer>(context);   // 將 Consumer 綁定到此佇列
            });
        }
    }

}
