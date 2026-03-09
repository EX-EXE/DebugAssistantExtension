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
internal class VsDebuggerEventsServiceHub(
    VisualStudioExtensibility visualStudioExtensibility,
    MemoryVisualizerService memoryVisualizerService)
    : IVsDebuggerEventsServiceHub
{
    public ValueTask<int> OnModeChangeAsync(VsDebuggerDebugModeInfo info, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(0);
    }

    [VisualStudioContribution]
    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(
            IVsDebuggerEventsServiceHub.Configuration.ServiceName,
            IVsDebuggerEventsServiceHub.Configuration.ServiceVersion,
            typeof(VsDebuggerEventsServiceHub))
        {
            ServiceAudience = BrokeredServiceAudience.Local,
        };

}
