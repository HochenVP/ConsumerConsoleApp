namespace ConsumerConsoleApp.Queue
{
    public static class QueueKeyMapper
    {
        public static string ToQueueName(this QueueKey key) => key switch
        {
            QueueKey.AllPhotoOriginalAnalysis => "photo.analysis.all.command",

            _ => key.ToString().ToLower()
        };
    }
}
