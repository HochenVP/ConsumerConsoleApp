using ConsumerConsoleApp.Helper;
using Contract.Events;
using MassTransit;

namespace ConsumerConsoleApp
{
    public static class MessagePusher
    {
        public static async Task PushTestMessageAsync(int count)
        {
            // 1. 建立 Bus
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });

            await busControl.StartAsync();
            try
            {
                var queueName = QueueNameHelper.GetQueueName(nameof(AllPhotoOriginalAnalysis));
                var sendEndpoint = await busControl.GetSendEndpoint(new Uri($"rabbitmq://localhost/{queueName}"));

                for (int i = 1; i <= count; i++)
                {
                    var testEvent = new AllPhotoOriginalAnalysis
                    {
                        FaceFrontPhotoGuid = "75641a08-7579-45f4-b01a-2f91f3573381",
                        Left45DegreeAnglePhotoGuid = "dba220f5-adb5-4245-814e-158a95f3e198",
                        Left90DegreeAnglePhotoGuid = "27eaa21d-a324-4ee9-a1c8-63c20001f07a",
                        Right45DegreeAnglePhotoGuid = "a94b07ce-b1b5-436e-bbfb-e08720917f81",
                        Right90DegreeAnglePhotoGuid = "89e8aa6c-e7ca-4323-9131-f57b8c68ff6f",
                        AnalysisPurpose = "online",
                        CustomerNo = "11111"
                    };
                    await sendEndpoint.Send(testEvent);
                    Console.WriteLine($"Test message {i} pushed to queue.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await busControl.StopAsync();
            }
        }
    }
}
