using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using ConsumerConsoleApp.Config;

namespace ConsumerConsoleApp.Consumers
{
    public class ApiForwardingConsumer<T> : IConsumer<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiForwardingConsumer<T>> _logger;
        private readonly IOptionsSnapshot<ApiForwardingConfig> _options;

        public ApiForwardingConsumer
        (
            HttpClient httpClient,
            ILogger<ApiForwardingConsumer<T>> logger,
            IOptionsSnapshot<ApiForwardingConfig> options
        )
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // 延長 timeout
            _logger = logger;
            _options = options;
        }

        public async Task Consume(ConsumeContext<T> context)
        {
            Console.WriteLine(context.MessageId);

            var _config = _options.Get(typeof(T).Name);

            var apiEndpoint = _config.Url;
            var httpMethod = _config.Method.ToUpperInvariant();

            var json = JsonSerializer.Serialize(context.Message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            HttpResponseMessage response = httpMethod switch
            {
                "POST" => await _httpClient.PostAsync(apiEndpoint, content),
                "PUT"  => await _httpClient.PutAsync(apiEndpoint, content),
                "GET"  => await _httpClient.GetAsync(BuildGetUrl(apiEndpoint, context.Message)),
                _ => throw new NotSupportedException($"HTTP method '{httpMethod}' is not supported.")
            };

        }

        /// <summary>
        /// 將物件屬性轉成 query string 並組合到 URL
        /// </summary>
        private static string BuildGetUrl(string baseUrl, object message)
        {
            // 取得 message 物件的所有 public 屬性
            var props = message.GetType().GetProperties();

            // 將每個屬性轉成 "屬性名=屬性值" 的格式，並進行 URL 編碼，最後用 & 串接
            var query = string.Join("&", props
                .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(message)?.ToString() ?? "")}"));

            // 如果沒有任何屬性，直接回傳 baseUrl；否則將 query string 加在 baseUrl 後面
            return string.IsNullOrWhiteSpace(query) ? baseUrl : $"{baseUrl}?{query}";
        }
    }
}