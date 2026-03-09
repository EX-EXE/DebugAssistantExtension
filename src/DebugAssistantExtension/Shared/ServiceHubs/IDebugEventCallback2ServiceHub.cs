using MessagePack;
using Microsoft.ServiceHub.Framework;

namespace DebugAssistantExtension.Shared.ServiceHubs;

[MessagePackObject]
public record class DebugEventCallbackInfo
{
    [Key(0)]
    public Guid IDebugEngine2Id { get; init; } = Guid.Empty;
    [Key(1)]
    public Guid IDebugProgram2Id { get; init; } = Guid.Empty;
    [Key(2)]
    public Guid IDebugThread2Id { get; init; } = Guid.Empty;
    [Key(3)]
    public Guid IDebugEvent2Id { get; init; } = Guid.Empty;
    [Key(4)]
    public Guid RiidEventId { get; init; } = Guid.Empty;
    [Key(5)]
    public uint Attribute { get; init; } = 0;
}

public interface IDebugEventCallback2ServiceHub
{
    Task OnEventAsync(
        DebugEventCallbackInfo info,
        CancellationToken cancellationToken);

    public static class Configuration
    {
        public const string ServiceName = "DebugAssistantExtension.Shared.ServiceHubs.IDebugEventCallback2ServiceHub";
        public const int MajorVersion = 1;
        public const int MinorVersion = 0;
        public static readonly Version ServiceVersion = new(MajorVersion, MinorVersion);

        public static readonly ServiceMoniker ServiceMoniker = new(ServiceName, ServiceVersion);

        public static ServiceRpcDescriptor ServiceDescriptor => new ServiceJsonRpcDescriptor(
            ServiceMoniker,
            ServiceJsonRpcDescriptor.Formatters.MessagePack,
            ServiceJsonRpcDescriptor.MessageDelimiters.BigEndianInt32LengthHeader);
    }
}
