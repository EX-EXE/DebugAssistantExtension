using DebugAssistantExtension.Shared.ServiceHubs;
using DebugAssistantExtension.Shared.Services;
using DebugAssistantExtension.VSExtensibility.MemoryVisualizers;
using DebugAssistantExtension.VSExtensibility.Services;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.RpcContracts.Notifications;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.VSExtensibility.ServiceHubs;

[VisualStudioContribution]
internal class VsCppDebugServiceHub(
    VisualStudioExtensibility visualStudioExtensibility,
    MemoryVisualizerService memoryVisualizerService)
    : IVsCppDebugServiceHub
{
    public async Task OnVsCppDebugUIVisualizerAsync(
        VsCppDebugUIVisualizerInfo info,
        CancellationToken cancellationToken)
    {
        // Service broker
        using var debugPropertyService = await visualStudioExtensibility.ServiceBroker.GetProxyServiceAsync<IDebugPropertyService>(
            IDebugPropertyService.Configuration.ServiceDescriptor,
            cancellationToken: cancellationToken);

        // Fetch memory info
        var memory = await debugPropertyService.Broker.FetchMemoryAsync(info.IDebugProperty3Id, cancellationToken);
        memoryVisualizerService.LatestMemoryInfo.Value = memory;

        // Show
        await visualStudioExtensibility.Shell().ShowToolWindowAsync<MemoryVisualizerToolWindow>(
            activate: true,
            cancellationToken);

    }

    [VisualStudioContribution]
    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(
            IVsCppDebugServiceHub.Configuration.ServiceName,
            IVsCppDebugServiceHub.Configuration.ServiceVersion,
            typeof(VsCppDebugServiceHub))
        {
            ServiceAudience = BrokeredServiceAudience.Local,
        };
}
