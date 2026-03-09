using DebugAssistantExtension.Shared.Brokers;
using DebugAssistantExtension.VSSDK.MemoryVisualizers;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.VSSDK;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(VSSDKEntrypoint.PackageGuidString)]
[ProvideService(typeof(SMemoryVisualizerService), ServiceName = nameof(MemoryVisualizerService), IsAsyncQueryable = true)]
[ProvideBrokeredServiceHubService(IBridgeBroker.Configuration.ServiceName, Audience = ServiceAudience.Local | ServiceAudience.RemoteExclusiveClient)]
public sealed class VSSDKEntrypoint : AsyncPackage
{
    public const string PackageGuidString = "F387AC7C-A421-43AA-A4F7-1391F33204B1";

    protected override async Task InitializeAsync(
        CancellationToken cancellationToken, 
        IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);

        this.AddService(typeof(SMemoryVisualizerService), async (sc, ct, st) =>
        {
            var service = new MemoryVisualizerService(this);
            await service.InitializeAsync(cancellationToken);
            return service;
        }, true);
    }
}
