using DebugAssistantExtension.Extensions;
using DebugAssistantExtension.Shared.Services;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Evaluation;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Services;

internal class DebugPropertyService(
    DebugObjectService debugObjectService)
    : IDebugPropertyService
{

    public async ValueTask<DebugPropertyMemoryInfo> FetchMemoryAsync(
        Guid iDebugPropety3Id,
        CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!debugObjectService.IDebugProperty3Dictionary.TryGetValue(iDebugPropety3Id, out var idebugProperty3))
        {
            throw new InvalidOperationException($"Could not find IDebugProperty3 for id {iDebugPropety3Id}");
        }

        if (!idebugProperty3.TryGetProperty2(
            enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
            10,
            0,
            out var outputDebugPropertyInfo))
        {
            throw new InvalidOperationException($"TryGetProperty2 failed for IDebugProperty3 with id {iDebugPropety3Id}");
        }

        var evaluationResult = DkmSuccessEvaluationResult.ExtractFromProperty(idebugProperty3);
        if (evaluationResult == null)
        {
            throw new InvalidOperationException($"Could not extract DkmSuccessEvaluationResult from IDebugProperty3 with id {iDebugPropety3Id}");
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
        return new DebugPropertyMemoryInfo() { Memory = bytesReturned };
    }
}
