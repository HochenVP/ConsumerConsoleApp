using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsumerConsoleApp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostEnvironment _hostEnvironment; // 注入 IHostEnvironment 來獲取環境名稱
        private readonly IConfiguration _configuration; // 注入 IConfiguration 來獲取配置值和命令行參數

        // 建構子：透過依賴注入取得所需的服務
        public Worker(ILogger<Worker> logger, IHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumerWorkerService 服務啟動於: {time}", DateTimeOffset.Now);

            // 從 IHostEnvironment 讀取應用程式環境名稱
            var environmentName = _hostEnvironment.EnvironmentName;
            _logger.LogInformation("從 IHostEnvironment 讀取到的應用程式環境: {env}", environmentName);

            // 從 IConfiguration 讀取 ASPNETCORE_ENVIRONMENT 環境變數的值 (通常與 IHostEnvironment.EnvironmentName 一致)
            var aspnetCoreEnvFromConfig = _configuration["ASPNETCORE_ENVIRONMENT"];
            _logger.LogInformation("從配置 (ASPNETCORE_ENVIRONMENT) 讀取到的環境變數: {env}", aspnetCoreEnvFromConfig ?? "未設定");

            // 從 IConfiguration 讀取通過命令行傳遞的自定義 "environment" 參數
            // 如果在 sc create 命令中傳遞了 --environment=Test，這裡會讀到 "Test"
            var customEnvironmentArg = _configuration["environment"];
            _logger.LogInformation("從命令行參數 (--environment) 讀取到的環境: {env}", customEnvironmentArg ?? "未指定");

            // 這裡可以放置一些服務啟動後立即執行的初始化邏輯
            // 例如：檢查連接、執行一次性數據準備等。

            // 由於 MassTransit 已經作為 HostedService 運行並處理訊息消費，
            // 這個 ExecuteAsync 方法的主要作用是保持服務運行，並可以在這裡放置週期性任務或心跳日誌。
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker 服務正在運行，等待 MassTransit 處理訊息... {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // 每分鐘記錄一次心跳日誌
            }

            _logger.LogInformation("ConsumerWorkerService 服務已停止於: {time}", DateTimeOffset.Now);
        }
    }
}
