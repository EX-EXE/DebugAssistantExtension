using DebugAssistantExtension.Extensions;
using DebugAssistantExtension.Services;
using DebugAssistantExtension.Shared.ServiceHubs;
using DebugAssistantExtension.Shared.Services;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Evaluation;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

namespace DebugAssistantExtension.MemoryVisualizers;

[Guid("89B79ADC-DA27-499C-A437-88138475AD37")]
public interface SMemoryVisualizerService
{
}

internal class MemoryVisualizerService(
    IAsyncServiceProvider asyncServiceProvider,
    DebugObjectService debugObjectService
    ) : SMemoryVisualizerService, IVsCppDebugUIVisualizer
{
    public int DisplayValue(uint ownerHwnd, uint visualizerId, IDebugProperty3 pDebugProperty)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var idebugProperty3Id = Guid.NewGuid();
            debugObjectService.IDebugProperty3Dictionary.TryAdd(idebugProperty3Id, pDebugProperty);

            using var proxy = await asyncServiceProvider.GetProxyServiceAsync<IVsCppDebugServiceHub>(
                IVsCppDebugServiceHub.Configuration.ServiceDescriptor,
                default);
            await proxy.Broker.OnVsCppDebugUIVisualizerAsync(
                new VsCppDebugUIVisualizerInfo()
                {
                    OwnerHwnd = ownerHwnd,
                    VisualizerId = visualizerId,
                    IDebugProperty3Id = idebugProperty3Id,
                }, default);
        });
        return 0;
    }

    private async Task DisplayValueAsync(
        uint ownerHwnd,
        uint visualizerId,
        IDebugProperty3 pDebugProperty)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!pDebugProperty.TryGetProperty2(
            enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
            10,
            0,
            out var outputDebugPropertyInfo))
        {
            return;
        }

        var evaluationResult = DkmSuccessEvaluationResult.ExtractFromProperty(pDebugProperty);
        if (evaluationResult == null)
        {
            return;
        }


        var stackFrame = evaluationResult.StackFrame;
        var language = evaluationResult.Language;
        var expr = DkmLanguageExpression.Create(
            language,
            DkmEvaluationFlags.TreatAsExpression,
            $"sizeof({outputDebugPropertyInfo.bstrFullName})",
            null);
        var workList = DkmWorkList.Create(null);
        var inspectionContext = evaluationResult.InspectionContext;

        var completionSource = new TaskCompletionSource<DkmEvaluateExpressionAsyncResult>();
        inspectionContext.EvaluateExpression(
            workList,
            expr,
            stackFrame,
            (result) =>
            {
                var a = result.ResultObject.FullName;
                completionSource.TrySetResult(result);
            });
        workList.Execute();




        outputDebugPropertyInfo.pProperty.TryGetMemoryBytes(
            2048,
            0x0,
            out var bytesReturned);

        await DoSomethingAsync(pDebugProperty);
        //var vm = new MemoryVisualizerViewModel();
        //var v = new MemoryVisualizerWindow(vm);
        //v.ShowDialog();

    }

    private async Task DoSomethingAsync(IDebugProperty3 pDebugProperty)
    {
        //var serviceBrokerContainer = await asyncServiceProvider.GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
        //var serviceBroker = serviceBrokerContainer.GetFullAccessServiceBroker();
        //IVsCppDebugBroker? myBrokeredServiceProxy = null;
        //try
        //{
        //    myBrokeredServiceProxy = await serviceBroker.GetProxyAsync<IVsCppDebugBroker>(IVsCppDebugBroker.Configuration.ServiceDescriptor, default);

        //    // await myBrokeredServiceProxy.TestAsync(pDebugProperty, default);

        //    Assumes.NotNull(myBrokeredServiceProxy);
        //    await myBrokeredServiceProxy.StartReportingProgressAsync("Doing some work", default);

        //    // Simulate doing some work.
        //    await Task.Delay(10000, default);

        //    await myBrokeredServiceProxy.StopReportingProgressAsync(default);
        //}
        //finally
        //{
        //    (myBrokeredServiceProxy as IDisposable)?.Dispose();
        //}
    }
}
