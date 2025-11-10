namespace ConsumerConsoleApp.Queue
{
    public static class QueueKeyExtensions
    {
        /// <summary>
        /// 將QueueKey轉換為RabbitMQ的Queue名稱
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ToQueueName(this QueueKey key)
        {
            return $"{key.ToString().ToLower()}-queue";
        }
    }
}
