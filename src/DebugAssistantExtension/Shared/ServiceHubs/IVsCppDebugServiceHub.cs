using MessagePack;
using Microsoft.ServiceHub.Framework;

namespace DebugAssistantExtension.Shared.ServiceHubs;

[MessagePackObject]
public record class VsCppDebugUIVisualizerInfo
{
    [Key(0)]
    public uint OwnerHwnd { get; init; } = 0;
    [Key(1)]
    public uint VisualizerId { get; init; } = 0;
    [Key(2)]
    public Guid IDebugProperty3Id { get; init; } = Guid.Empty;
}

public interface IVsCppDebugServiceHub
{
    Task OnVsCppDebugUIVisualizerAsync(
        VsCppDebugUIVisualizerInfo info,
        CancellationToken cancellationToken);

    public static class Configuration
    {
        public const string ServiceName = "DebugAssistantExtension.Shared.ServiceHubs.IVsCppDebugServiceHub";
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
