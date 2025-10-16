namespace ConsumerConsoleApp.Config
{
    public class ApiForwardingConfig
    {
        public string BaseUrlKey { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = "POST";
        public string Url { get; set; } = string.Empty;
    }
}