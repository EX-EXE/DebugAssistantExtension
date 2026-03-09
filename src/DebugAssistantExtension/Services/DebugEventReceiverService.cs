

using DebugAssistantExtension.Extensions;
using DebugAssistantExtension.Shared.ServiceHubs;
using EnvDTE;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DebugAssistantExtension.Services;

internal class DebugEventReceiverService(
    Microsoft.VisualStudio.Shell.IAsyncServiceProvider asyncServiceProvider,
    DebugObjectService debugObjectService)
    : IAsyncDisposable
    , IDebugEventCallback2
    , IVsDebuggerEvents
{
    private uint? debuggerEventsCookie = null;

    internal async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var dteInstance = await asyncServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
        var vsDebugger = (IVsDebugger?)Package.GetGlobalService(typeof(SVsShellDebugger));
        if (vsDebugger != null)
        {
            vsDebugger.AdviseDebugEventCallback(this);
            vsDebugger.AdviseDebuggerEvents(this, out var pdwCookie);
            debuggerEventsCookie = pdwCookie;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dteInstance = await asyncServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
        var vsDebugger = (IVsDebugger?)Package.GetGlobalService(typeof(SVsShellDebugger));
        if (vsDebugger != null)
        {
            vsDebugger.UnadviseDebugEventCallback(this);
            if (debuggerEventsCookie.HasValue)
            {
                vsDebugger.UnadviseDebuggerEvents(debuggerEventsCookie.Value);
            }
        }
    }


    public int Event(
        IDebugEngine2 pEngine,
        IDebugProcess2 pProcess,
        IDebugProgram2 pProgram,
        IDebugThread2 pThread,
        IDebugEvent2 pEvent,
        ref Guid riidEvent,
        uint dwAttrib)
    {
        var riidEventId = riidEvent;
            var engineId = Guid.Empty;
            if (pEngine != null)
            {
                engineId = Guid.NewGuid();
                debugObjectService.IDebugEngine2Dictionary.TryAdd(engineId, pEngine);
            }
            var processId = Guid.Empty;
            if (pProcess != null)
            {
                processId = Guid.NewGuid();
                debugObjectService.IDebugProcess2Dictionary.TryAdd(processId, pProcess);
            }
            var programId = Guid.Empty;
            if (pProgram != null)
            {
                programId = Guid.NewGuid();
                debugObjectService.IDebugProgram2Dictionary.TryAdd(programId, pProgram);
            }
            var threadId = Guid.Empty;
            if (pThread != null)
            {
                threadId = Guid.NewGuid();
                debugObjectService.IDebugThread2Dictionary.TryAdd(threadId, pThread);
            }
            var debugEventId = Guid.Empty;
            if (pEvent != null)
            {
                debugEventId = Guid.NewGuid();
                debugObjectService.IDebugEvent2Dictionary.TryAdd(debugEventId, pEvent);
            }

        _ = Task.Run(async () =>
        {
            using var proxy = await asyncServiceProvider.GetProxyServiceAsync<IDebugEventCallback2ServiceHub>(
                IDebugEventCallback2ServiceHub.Configuration.ServiceDescriptor,
                default);
            await proxy.Broker.OnEventAsync(new DebugEventCallbackInfo()
            {
                IDebugEngine2Id = engineId,
                IDebugProgram2Id = programId,
                IDebugThread2Id = threadId,
                IDebugEvent2Id = debugEventId,
                RiidEventId = riidEventId,
                Attribute = dwAttrib,
            }, cancellationToken: default);
        });
        return 0;
    }

    public int OnModeChange(DBGMODE dbgmodeNew)
    {
        _ = Task.Run(async () =>
        {
            var idebugProperty3Id = Guid.NewGuid();
            using var proxy = await asyncServiceProvider.GetProxyServiceAsync<IVsDebuggerEventsServiceHub>(
                IVsDebuggerEventsServiceHub.Configuration.ServiceDescriptor,
                default);
            await proxy.Broker.OnModeChangeAsync(
                new VsDebuggerDebugModeInfo()
                {
                    DebugMode = dbgmodeNew switch
                    {
                        DBGMODE.DBGMODE_Design => VsDebuggerDebugMode.Design,
                        DBGMODE.DBGMODE_Break => VsDebuggerDebugMode.Break,
                        DBGMODE.DBGMODE_Run => VsDebuggerDebugMode.Run,
                        DBGMODE.DBGMODE_Enc => VsDebuggerDebugMode.Enc,
                        DBGMODE.DBGMODE_EncMask => VsDebuggerDebugMode.EncMask,
                        _ => throw new NotImplementedException($"{dbgmodeNew}")
                    }
                }, default);
        });
        return 0;
    }
}
