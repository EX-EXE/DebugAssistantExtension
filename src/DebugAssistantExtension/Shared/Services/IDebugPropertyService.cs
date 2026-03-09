using DebugAssistantExtension.Shared.ServiceHubs;
using MessagePack;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Debugger.Interop;
using Nerdbank.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Shared.Services;

[MessagePackObject]
public record class DebugPropertyMemoryInfo
{
    [Key(0)]
    public byte[] Memory { get; init; } = Array.Empty<byte>();
}

[MessagePackObject]
public record class DebugFrameInfo
{
    [Key(0)]
    public Guid FrameInfoId { get; init; } = Guid.Empty;
}


[MessagePackObject]
public record class DebugPropertyInfo
{
    [Key(0)]
    public Guid DebugPropertyInfoId { get; init; } = Guid.Empty;
    [Key(1)]
    public string FullName { get; init; } = "";
    [Key(2)]
    public string Name { get; init; } = "";
    [Key(3)]
    public string Type { get; init; } = "";
    [Key(4)]
    public string Value { get; init; } = "";
    [Key(5)]
    public enum_DEBUGPROP_INFO_FLAGS Flags { get; init; } = enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NONE;
    [Key(6)]
    public enum_DBG_ATTRIB_FLAGS Attribites { get; init; } = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_NONE;
}


[MessagePackObject]
public record class DebugExpressionContextInfo
{
    [Key(0)]
    public Guid DebugExpressionContext2Id { get; init; } = Guid.Empty;
}

[Flags]
public enum DebugPropertyParseType : ulong
{
    EXPRESSION = 1,
    FUNCTION_AS_ADDRESS = 2,
    DESIGN_TIME_EXPR_EVAL = 0x1000
}
[Flags]
public enum DebugPropertyEvalType : ulong
{
    RETURNVALUE = 2,
    NOSIDEEFFECTS = 4,
    ALLOWBPS = 8,
    ALLOWERRORREPORT = 0x10,
    FUNCTION_AS_ADDRESS = 0x40,
    NOFUNCEVAL = 0x80,
    NOEVENTS = 0x1000,
    DESIGN_TIME_EXPR_EVAL = 0x2000,
    ALLOW_IMPLICIT_VARS = 0x4000
}

[MessagePackObject]
public record class DebugPropertyEvaluateInfo
{
    [Key(0)]
    public Guid DebugProperty2Id { get; init; } = Guid.Empty;
}

[MessagePackObject]
public record class DebugPropertyMemoryBytes
{
    [Key(0)]
    public byte[] Memory { get; init; } = Array.Empty<byte>();
}

public interface IDebugPropertyService
{
    ValueTask<DebugPropertyMemoryInfo> FetchMemoryAsync(
        Guid iDebugPropety3Id,
        CancellationToken cancellationToken);

    public static class Configuration
    {
        public const string ServiceName = "DebugAssistantExtension.Shared.Services.IDebugPropertyService";
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