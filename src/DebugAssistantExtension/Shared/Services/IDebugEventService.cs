using DebugAssistantExtension.Shared.ServiceHubs;
using MessagePack;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Debugger.Interop;
using Nerdbank.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Shared.Services;

//[MessagePackObject]
//public record class DebugPropertyMemoryInfo
//{
//    [Key(0)]
//    public byte[] Memory { get; init; } = Array.Empty<byte>();
//}

public interface IDebugEventService
{
    ValueTask<DebugPropertyEvaluateInfo> EvaluatePropertyAsync(Guid debugExpressionContext2Id, string pszCode, DebugPropertyParseType dwParseFlags, uint nRadix, DebugPropertyEvalType dwEvalFlags, uint dwTimeout, CancellationToken cancellationToken);
    ValueTask<uint> FetchCurrentStackFrameDepthAsync(CancellationToken cancellationToken);
    ValueTask<DebugExpressionContextInfo> FetchExpressionContextAsync(Guid frameInfoId, CancellationToken cancellationToken);
    IAsyncEnumerable<DebugPropertyInfo> FetchPropertiesAsync(Guid frameInfoId, Guid filter, CancellationToken cancellationToken);
    IAsyncEnumerable<DebugPropertyInfo> FetchPropertyChildrenAsync(Guid debugPropertyInfoId, Guid filter, enum_DBG_ATTRIB_FLAGS attributeFilter, string? nameFilter, CancellationToken cancellationToken);
    ValueTask<DebugPropertyInfo> FetchPropertyInfoAsync(Guid debugProperty2Id, enum_DEBUGPROP_INFO_FLAGS flags, CancellationToken cancellationToken);
    ValueTask<DebugPropertyMemoryBytes> FetchPropertyMemoryBytesAsync(Guid debugProperty2Id, long limitReadSize, byte? endByte, CancellationToken cancellationToken);
    IAsyncEnumerable<DebugFrameInfo> FetchStackFramesAsync(Guid iDebugThread2Id, CancellationToken cancellationToken);
    ValueTask<DebugFrameInfo> FetchStackFrameAsync(Guid iDebugThread2Id,uint depth, CancellationToken cancellationToken);


    public static class Configuration
    {
        public const string ServiceName = "DebugAssistantExtension.Shared.Services.IDebugEventService";
        public const string ServiceVersionString = "1.0";
        public static readonly Version ServiceVersion = new(ServiceVersionString);

        public static readonly ServiceMoniker ServiceMoniker = new(ServiceName, ServiceVersion);


        public static readonly ServiceRpcDescriptor ServiceDescriptor = new ServiceJsonRpcDescriptor(
            ServiceMoniker,
            clientInterface: null,
            ServiceJsonRpcDescriptor.Formatters.MessagePack,
            ServiceJsonRpcDescriptor.MessageDelimiters.BigEndianInt32LengthHeader,
            new MultiplexingStream.Options { ProtocolMajorVersion = 3 })
            .WithExceptionStrategy(StreamJsonRpc.ExceptionProcessing.ISerializable);
    }
}