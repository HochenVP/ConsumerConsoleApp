using ConsumerConsoleApp.Evemts;
using ConsumerConsoleApp.Helper;
using MassTransit;

namespace ConsumerConsoleApp.Consumers
{
    /// <summary>
    /// 消費者（Consumer），用於處理 <see cref="AllPhotoOriginalAnalysisCommand"/> 訊息。
    /// 當訊息總線（Message Bus）接收到該命令時，會由此類別自動觸發處理邏輯。
    /// </summary>
    public class AllPhotoOriginalAnalysisConsumer : IConsumer<AllPhotoOriginalAnalysisCommand>
    {
        private readonly ApiForwardingHelper _apiForwardingHelper;

        public AllPhotoOriginalAnalysisConsumer
        (
            ApiForwardingHelper apiForwardingHelper
        )
        {
            _apiForwardingHelper = apiForwardingHelper;
        }


        public async Task Consume(ConsumeContext<AllPhotoOriginalAnalysisCommand> context)
        {
            await _apiForwardingHelper.ForwardCommandAsync(
                nameof(AllPhotoOriginalAnalysisConsumer),
                context.Message.Para
            );
        }
    }
}
