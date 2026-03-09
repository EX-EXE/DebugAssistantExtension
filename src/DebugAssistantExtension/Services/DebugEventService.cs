using DebugAssistantExtension.Extensions;
using DebugAssistantExtension.Shared.Services;
using EnvDTE;
using EnvDTE90a;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Evaluation;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Services;

internal class DebugEventService(
    DebugObjectService debugObjectService,
    DTE dte)
    : IDebugEventService
{
    public async ValueTask<uint> FetchCurrentStackFrameDepthAsync(CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var currentStackFrame = dte.Debugger.CurrentStackFrame;
        if (currentStackFrame is StackFrame2 stackFrame2)
        {
            return 0 < stackFrame2.Depth
                ? stackFrame2.Depth - 1
                : 0;
        }
        throw new InvalidOperationException(
            $"CurrentStackFrame is null or not StackFrame2. CurrentStackFrame: {currentStackFrame?.GetType().FullName ?? "null"}");
    }

    public async ValueTask<DebugFrameInfo> FetchStackFrameAsync(
        Guid iDebugThread2Id,
        uint depth,
        CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!debugObjectService.IDebugThread2Dictionary.TryGetValue(iDebugThread2Id, out var iDebugThread2))
        {
            throw new InvalidOperationException($"Could not find IDebugThread2 for id {iDebugThread2}");
        }
        FRAMEINFO? frameInfo = iDebugThread2.EnumerateFrameInfo(
            enum_FRAMEINFO_FLAGS.FIF_FRAME,
            10)
            .Skip((int)depth)
            .FirstOrDefault();
        if (frameInfo == null)
        {
            throw new InvalidOperationException(
                $"Could not get FrameInfo for IDebugThread2 with id {iDebugThread2Id} at depth {depth}");
        }

        var id = Guid.NewGuid();
        debugObjectService.FrameInfoDictionary.TryAdd(id, frameInfo.Value);
        return new DebugFrameInfo()
        {
            FrameInfoId = id,
        };
    }

    public async IAsyncEnumerable<DebugFrameInfo> FetchStackFramesAsync(
        Guid iDebugThread2Id,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!debugObjectService.IDebugThread2Dictionary.TryGetValue(iDebugThread2Id, out var iDebugThread2))
        {
            throw new InvalidOperationException($"Could not find IDebugThread2 for id {iDebugThread2}");
        }

        foreach (var frameInfo in iDebugThread2.EnumerateFrameInfo(
            enum_FRAMEINFO_FLAGS.FIF_FRAME,
            10))
        {
            var frameInfoId = Guid.NewGuid();
            debugObjectService.FrameInfoDictionary.TryAdd(frameInfoId, frameInfo);
            yield return new DebugFrameInfo()
            {
                FrameInfoId = frameInfoId,
            };
        }
    }

    public async IAsyncEnumerable<DebugPropertyInfo> FetchPropertiesAsync(
        Guid frameInfoId,
        Guid filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (!debugObjectService.FrameInfoDictionary.TryGetValue(frameInfoId, out var frameInfo))
        {
            throw new InvalidOperationException($"Could not find FrameInfo for id {frameInfoId}");
        }
        FRAMEINFO? info = frameInfo;
        foreach (var propertyInfo in info.EnumerateProperties(
            enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
            10,
            filter,
            0))
        {
            var id = Guid.NewGuid();
            debugObjectService.DebugPropertyInfoDictionary.TryAdd(id, propertyInfo);
            yield return new DebugPropertyInfo()
            {
                DebugPropertyInfoId = id,
                FullName = propertyInfo.bstrFullName,
                Name = propertyInfo.bstrName,
                Type = propertyInfo.bstrType,
                Value = propertyInfo.bstrValue,
                Flags = (enum_DEBUGPROP_INFO_FLAGS)propertyInfo.dwFields,
                Attribites = (enum_DBG_ATTRIB_FLAGS)propertyInfo.dwAttrib,
            };
        }
    }

    public async IAsyncEnumerable<DebugPropertyInfo> FetchPropertyChildrenAsync(
        Guid debugPropertyInfoId,
        Guid filter,
        enum_DBG_ATTRIB_FLAGS attributeFilter,
        string? nameFilter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (!debugObjectService.DebugPropertyInfoDictionary.TryGetValue(debugPropertyInfoId, out var debugPropertyInfo))
        {
            throw new InvalidOperationException($"Could not find DebugPropertyInfo for id {debugPropertyInfoId}");
        }

        foreach (var propertyInfo in debugPropertyInfo.pProperty.EnumerateChildren(
            enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
            10,
            filter,
            (enum_DBG_ATTRIB_FLAGS)attributeFilter,
            nameFilter,
            0))
        {
            var id = Guid.NewGuid();
            debugObjectService.DebugPropertyInfoDictionary.TryAdd(id, propertyInfo);
            yield return new DebugPropertyInfo()
            {
                DebugPropertyInfoId = id,
                FullName = propertyInfo.bstrFullName,
                Name = propertyInfo.bstrName,
                Type = propertyInfo.bstrType,
                Value = propertyInfo.bstrValue,
                Flags = (enum_DEBUGPROP_INFO_FLAGS)propertyInfo.dwFields,
                Attribites = (enum_DBG_ATTRIB_FLAGS)propertyInfo.dwAttrib,
            };
        }
    }

    public async ValueTask<DebugExpressionContextInfo> FetchExpressionContextAsync(
        Guid frameInfoId,
        CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (!debugObjectService.FrameInfoDictionary.TryGetValue(frameInfoId, out var frameInfo))
        {
            throw new InvalidOperationException($"Could not find FrameInfo for id {frameInfoId}");
        }
        if (frameInfo.m_pFrame == null ||
            frameInfo.m_pFrame.GetExpressionContext(out var debugExpressionContext) != 0)
        {
            throw new InvalidOperationException(
                $"Could not get expression context from FrameInfo with id {frameInfoId}");
        }

        var id = Guid.NewGuid();
        debugObjectService.IDebugExpressionContext2Dictionary.TryAdd(id, debugExpressionContext);
        return new DebugExpressionContextInfo()
        {
            DebugExpressionContext2Id = id,
        };
    }

    public async ValueTask<DebugPropertyEvaluateInfo> EvaluatePropertyAsync(
        Guid debugExpressionContext2Id,
        string pszCode,
        DebugPropertyParseType dwParseFlags,
        uint nRadix,
        DebugPropertyEvalType dwEvalFlags,
        uint dwTimeout,
        CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (!debugObjectService.IDebugExpressionContext2Dictionary.TryGetValue(debugExpressionContext2Id, out var debugExpressionContextInfo))
        {
            throw new InvalidOperationException($"Could not find DebugExpressionContextInfo for id {debugExpressionContext2Id}");
        }

        if (!debugExpressionContextInfo.TryGetEvaluateSync(
            pszCode,
            (enum_PARSEFLAGS)dwParseFlags,
            nRadix,
            (enum_EVALFLAGS)dwEvalFlags,
            dwTimeout,
            out var outputDebugProperty))
        {
            throw new InvalidOperationException(
                $"Evaluate failed for DebugExpressionContextInfo with id {debugExpressionContext2Id}. Code: {pszCode}, ParseFlags: {dwParseFlags}, Radix: {nRadix}, EvalFlags: {dwEvalFlags}, Timeout: {dwTimeout}");
        }

        var id = Guid.NewGuid();
        debugObjectService.DebugProperty2Dictionary.TryAdd(id, outputDebugProperty);

        return new DebugPropertyEvaluateInfo()
        {
            DebugProperty2Id = id,
        };
    }

    public async ValueTask<DebugPropertyInfo> FetchPropertyInfoAsync(
        Guid debugProperty2Id,
        enum_DEBUGPROP_INFO_FLAGS flags,
        CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (!debugObjectService.DebugProperty2Dictionary.TryGetValue(debugProperty2Id, out var debugProperty2D))
        {
            throw new InvalidOperationException($"Could not find DebugProperty2Id for id {debugProperty2Id}");
        }

        if (!debugProperty2D.TryGetProperty(
            (enum_DEBUGPROP_INFO_FLAGS)flags,
            10,
            0,
            out var outputDebugPropertyInfo))
        {
            throw new InvalidOperationException(
                $"TryGetProperty failed for DebugProperty2 with id {debugProperty2Id}. Flags: {flags}");
        }
        return new DebugPropertyInfo()
        {
            DebugPropertyInfoId = debugProperty2Id,
            FullName = outputDebugPropertyInfo.bstrFullName,
            Name = outputDebugPropertyInfo.bstrName,
            Type = outputDebugPropertyInfo.bstrType,
            Value = outputDebugPropertyInfo.bstrValue,
            Flags = (enum_DEBUGPROP_INFO_FLAGS)outputDebugPropertyInfo.dwFields,
            Attribites = (enum_DBG_ATTRIB_FLAGS)outputDebugPropertyInfo.dwAttrib,
        };
    }

    public async ValueTask<DebugPropertyMemoryBytes> FetchPropertyMemoryBytesAsync(
        Guid debugProperty2Id,
        long limitReadSize,
        byte? endByte,
        CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (!debugObjectService.DebugProperty2Dictionary.TryGetValue(debugProperty2Id, out var debugProperty2D))
        {
            throw new InvalidOperationException($"Could not find DebugProperty2Id for id {debugProperty2Id}");
        }

        if (!debugProperty2D.TryGetMemoryBytes(
            limitReadSize,
            endByte,
            out var bytesReturned))
        {
            throw new InvalidOperationException(
                $"TryGetMemoryBytes failed for DebugProperty2 with id {debugProperty2Id}. LimitReadSize: {limitReadSize}, EndByte: {endByte}");
        }
        return new DebugPropertyMemoryBytes()
        {
            Memory = bytesReturned,
        };
    }
}
