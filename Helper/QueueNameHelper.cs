namespace ConsumerConsoleApp.Helper
{
    public static class QueueNameHelper
    {
        public static string GetQueueName(string eventName)
            => $"{eventName.ToLower()}-queue";
    }
}