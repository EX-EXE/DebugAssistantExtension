using DebugAssistantExtension.MemoryVisualizers;
using DebugAssistantExtension.Services;
using DebugAssistantExtension.Shared.ServiceHubs;
using DebugAssistantExtension.Shared.Services;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DebugAssistantExtension;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
[Guid(VSSDKEntrypoint.PackageGuidString)]
[ProvideService(typeof(SMemoryVisualizerService), ServiceName = nameof(MemoryVisualizerService), IsAsyncQueryable = true)]
[ProvideBrokeredService(
    IDebugPropertyService.Configuration.ServiceName,
    IDebugPropertyService.Configuration.ServiceVersionString,
    Audience = ServiceAudience.Local | ServiceAudience.PublicSdk)]
[ProvideBrokeredService(
    IDebugEventService.Configuration.ServiceName,
    IDebugEventService.Configuration.ServiceVersionString,
    Audience = ServiceAudience.Local | ServiceAudience.PublicSdk)]
[ProvideBrokeredServiceHubService(IVsCppDebugServiceHub.Configuration.ServiceName, Audience = ServiceAudience.Local | ServiceAudience.RemoteExclusiveClient)]
[ProvideBrokeredServiceHubService(IDebugEventCallback2ServiceHub.Configuration.ServiceName, Audience = ServiceAudience.Local | ServiceAudience.RemoteExclusiveClient)]
[ProvideBrokeredServiceHubService(IVsDebuggerEventsServiceHub.Configuration.ServiceName, Audience = ServiceAudience.Local | ServiceAudience.RemoteExclusiveClient)]
public sealed class VSSDKEntrypoint : AsyncPackage
{
    public const string PackageGuidString = "F387AC7C-A421-43AA-A4F7-1391F33204B1";

    IDisposable? serviceBrokerRegistration;

    protected override async Task InitializeAsync(
        CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);

        this.AddService(typeof(DebugObjectService), async (sc, ct, st) =>
        {
            return await Task.FromResult(new DebugObjectService());
        }, true);

        this.AddService(typeof(SMemoryVisualizerService), async (sc, ct, st) =>
        {
            var debugObjectService = await this.GetServiceAsync(typeof(DebugObjectService)) as DebugObjectService;
            if (debugObjectService is null)
            {
                throw new InvalidOperationException("Required service 'DebugObjectService' could not be found while creating MemoryVisualizerService.");
            }
            var service = new MemoryVisualizerService(this, debugObjectService);
            return service;
        }, true);

        this.AddService(typeof(DebugEventReceiverService), async (sc, ct, st) =>
        {
            var debugObjectService = await this.GetServiceAsync(typeof(DebugObjectService)) as DebugObjectService;
            if (debugObjectService is null)
            {
                throw new InvalidOperationException("Required service 'DebugObjectService' could not be found while creating MemoryVisualizerService.");
            }
            var service = new DebugEventReceiverService(this, debugObjectService);
            return service;
        }, true);
        var debugEventReceiverService = await GetServiceAsync(typeof(DebugEventReceiverService)) as DebugEventReceiverService;
        if (debugEventReceiverService == null)
        {
            throw new InvalidOperationException("Failed to retrieve DebugEventReceiverService from service container. This service is required for the extension to function properly.");
        }
        await debugEventReceiverService.InitializeAsync(cancellationToken);


        var brokeredServiceContainer = await AsyncServiceProvider.GlobalProvider.GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
        if (brokeredServiceContainer == null)
        {
            throw new InvalidOperationException("Failed to retrieve SVsBrokeredServiceContainer.");
        }
        serviceBrokerRegistration = brokeredServiceContainer.Proffer(
            IDebugPropertyService.Configuration.ServiceDescriptor,
            async (moniker, options, serviceBroker, cancellationToken) =>
            {
                var debugObjectService = await this.GetServiceAsync(typeof(DebugObjectService)) as DebugObjectService;
                if (debugObjectService is null)
                {
                    throw new InvalidOperationException("Required service 'DebugObjectService' could not be found while creating MemoryVisualizerService.");
                }
                var service = new DebugPropertyService(debugObjectService);
                return service;
            });

        serviceBrokerRegistration = brokeredServiceContainer.Proffer(
            IDebugEventService.Configuration.ServiceDescriptor,
            async (moniker, options, serviceBroker, cancellationToken) =>
            {
                var dte = await this.GetServiceAsync(typeof(DTE)) as DTE;
                if (dte is null)
                {
                    throw new InvalidOperationException("Required service 'DebugObjectService' could not be found while creating MemoryVisualizerService.");
                }

                var debugObjectService = await this.GetServiceAsync(typeof(DebugObjectService)) as DebugObjectService;
                if (debugObjectService is null)
                {
                    throw new InvalidOperationException("Required service 'DebugObjectService' could not be found while creating MemoryVisualizerService.");
                }
                var service = new DebugEventService(debugObjectService, dte);
                return service;
            });

    }
}
