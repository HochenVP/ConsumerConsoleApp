using ConsumerConsoleApp.Events.Parameters;
using MassTransit;
namespace ConsumerConsoleApp.Evemts
{
    /// <summary>
    /// AllPhotoOriginalAnalysis 命令
    /// </summary>
    /// <param name="Para"></param>
    /// <remarks>
    /// 要使用MessageUrn
    /// </remarks>
    [MessageUrn("scheme:publish-command", useDefaultPrefix: false)]
    public record AllPhotoOriginalAnalysisCommand(AllPhotoAnalysisPara Para);
}
