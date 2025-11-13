using ConsumerConsoleApp.Events.Parameters;
using MassTransit;

namespace ConsumerConsoleApp.Events
{
    /// <summary>
    /// PhotoPipelineExecuteCommand
    /// </summary>
    /// <remarks>
    /// </remarks>
    [MessageUrn("scheme:publish-command", useDefaultPrefix: false)]
    public record PhotoPipelineExecuteCommand(PhotoPipelinePara PhotoPipelinePara);
}
