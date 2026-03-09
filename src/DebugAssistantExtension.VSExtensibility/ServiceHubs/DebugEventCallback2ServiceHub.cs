using DebugAssistantExtension.Shared;
using DebugAssistantExtension.Shared.ServiceHubs;
using DebugAssistantExtension.Shared.Services;
using DebugAssistantExtension.VSExtensibility.MemoryVisualizers;
using DebugAssistantExtension.VSExtensibility.Services;
using Microsoft.VisualStudio.Debugger.Interop;
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
internal class DebugEventCallback2ServiceHub(
    VisualStudioExtensibility visualStudioExtensibility,
    LocalVariablesService localVariablesService)
    : IDebugEventCallback2ServiceHub
{
    public async Task OnEventAsync(
        DebugEventCallbackInfo info,
        CancellationToken cancellationToken)
    {
        if (info.RiidEventId != GuidConstants.DebugChangeContext)
        {
            return;
        }

        localVariablesService.Items.Clear();

        // Service broker
        using var debugEventService = await visualStudioExtensibility.ServiceBroker.GetProxyServiceAsync<IDebugEventService>(
            IDebugEventService.Configuration.ServiceDescriptor,
            cancellationToken: cancellationToken);

        var currentStackFrameDepth = await debugEventService.Broker.FetchCurrentStackFrameDepthAsync(cancellationToken);
        var frameInfo = await debugEventService.Broker.FetchStackFrameAsync(info.IDebugThread2Id, currentStackFrameDepth, cancellationToken);

        var expressionContext = await debugEventService.Broker.FetchExpressionContextAsync(frameInfo.FrameInfoId, cancellationToken);

        await foreach (var propertyInfo in debugEventService.Broker.FetchPropertiesAsync(
            frameInfo.FrameInfoId,
            GuidConstants.FilterLocals,
            cancellationToken))
        {
            var indirectSize = "";
            var memoryHex = "";
            var memoryAscii = "";
            if (propertyInfo.Type.Contains("*"))
            {

                var evaluateSize = await debugEventService.Broker.EvaluatePropertyAsync(
                    expressionContext.DebugExpressionContext2Id,
                    $"sizeof(*{propertyInfo.FullName})",
                    DebugPropertyParseType.EXPRESSION,
                    10,
                    DebugPropertyEvalType.NOSIDEEFFECTS,
                    0,
                    cancellationToken);
                var resultSize = await debugEventService.Broker.FetchPropertyInfoAsync(
                    evaluateSize.DebugProperty2Id,
                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
                    cancellationToken);
                if (propertyInfo.Type.Contains("char"))
                {
                    var evaluateBytes = await debugEventService.Broker.EvaluatePropertyAsync(
                        expressionContext.DebugExpressionContext2Id,
                        propertyInfo.FullName,
                        DebugPropertyParseType.EXPRESSION,
                        10,
                        DebugPropertyEvalType.NOSIDEEFFECTS,
                        0,
                        cancellationToken);
                    var resultBytes = await debugEventService.Broker.FetchPropertyMemoryBytesAsync(
                        evaluateBytes.DebugProperty2Id,
                        2048,
                        0x0,
                        cancellationToken);
                    memoryHex = string.Join(" ", resultBytes.Memory.Select(b => b.ToString("X2")));
                    memoryAscii = string.Concat(resultBytes.Memory.Select(b => b >= 32 && b <= 126 ? (char)b : '.'));
                }
                else if (int.TryParse(indirectSize, out var indirectSizeNum))
                {
                    // Pointer Type
                    var requestReadSize = Math.Min(indirectSizeNum, 2048);
                    var evaluateBytes = await debugEventService.Broker.EvaluatePropertyAsync(
                        expressionContext.DebugExpressionContext2Id,
                        propertyInfo.FullName,
                        DebugPropertyParseType.EXPRESSION,
                        10,
                        DebugPropertyEvalType.NOSIDEEFFECTS,
                        0,
                        cancellationToken);
                    var resultBytes = await debugEventService.Broker.FetchPropertyMemoryBytesAsync(
                        evaluateBytes.DebugProperty2Id,
                        requestReadSize,
                        null,
                        cancellationToken);
                    memoryHex = string.Join(" ", resultBytes.Memory.Select(b => b.ToString("X2")));
                    memoryAscii = string.Concat(resultBytes.Memory.Select(b => b >= 32 && b <= 126 ? (char)b : '.'));

                }
            }

            localVariablesService.Items.Add(new LocalVariableInfo
            {
                Name = propertyInfo.Name,
                Value = propertyInfo.Value,
                Type = propertyInfo.Type,
                IndirectSize = indirectSize,
                MemoryHex = memoryHex,
                MemoryAscii = memoryAscii,
            });
        }
    }

    [VisualStudioContribution]
    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(
            IDebugEventCallback2ServiceHub.Configuration.ServiceName,
            IDebugEventCallback2ServiceHub.Configuration.ServiceVersion,
            typeof(DebugEventCallback2ServiceHub))
        {
            ServiceAudience = BrokeredServiceAudience.Local,
        };

}
