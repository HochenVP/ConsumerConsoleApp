using ConsumerConsoleApp.Config;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ConsumerConsoleApp.Helper
{
    /// <summary>
    /// 提供將任意 Command 轉發至指定 API 的共用工具方法。
    /// </summary>
    public class ApiForwardingHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiForwardingHelper> _logger;
        private readonly IOptionsSnapshot<ApiForwardingConfig> _options;

        public ApiForwardingHelper(
            HttpClient httpClient,
            ILogger<ApiForwardingHelper> logger,
            IOptionsSnapshot<ApiForwardingConfig> options)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            _logger = logger;
            _options = options;
        }

        /// <summary>
        /// 將指定命令物件序列化後，依據 optionsKey 轉發至對應的 API。
        /// </summary>
        /// <typeparam name="T">命令的型別。</typeparam>
        /// <param name="optionsKey">對應 appsettings 中的設定鍵名。</param>
        /// <param name="command">要轉發的命令物件。</param>
        /// <param name="context">可選，MassTransit 的 context 用於紀錄追蹤資訊。</param>
        public async Task ForwardCommandAsync<T>(
            string optionsKey,
            T command
        )
            where T : class
        {
            var config = _options.Get(optionsKey) ?? throw new InvalidOperationException($"找不到 {optionsKey} 的 API 轉發設定。");               
            var apiEndpoint = config.Url;
            var httpMethod = config.Method.ToUpperInvariant();

            _logger.LogInformation("轉發命令 {CommandType} 至 {Api} ({Method})", typeof(T).Name, apiEndpoint, httpMethod);
            
            var json = JsonSerializer.Serialize(command);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = httpMethod switch
                {
                    "POST" => await _httpClient.PostAsync(apiEndpoint, content),
                    "PUT" => await _httpClient.PutAsync(apiEndpoint, content),
                    "GET" => await _httpClient.GetAsync(BuildGetUrl(apiEndpoint, command)),
                    _ => throw new NotSupportedException($"HTTP method '{httpMethod}' 不支援。")
                };

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API 轉發失敗 ({StatusCode}): {Body}", response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "轉發命令 {CommandType} 發生例外", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 將物件屬性轉成 query string 並組合到 URL。
        /// </summary>
        private static string BuildGetUrl(string baseUrl, object message)
        {
            var props = message.GetType().GetProperties();
            var query = string.Join("&", props
                .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(message)?.ToString() ?? "")}"));
            return string.IsNullOrWhiteSpace(query) ? baseUrl : $"{baseUrl}?{query}";
        }
    }

}
