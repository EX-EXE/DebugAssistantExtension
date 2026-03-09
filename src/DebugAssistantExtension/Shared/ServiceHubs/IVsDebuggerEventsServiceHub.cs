using MessagePack;
using Microsoft.ServiceHub.Framework;

namespace DebugAssistantExtension.Shared.ServiceHubs;

public enum VsDebuggerDebugMode
{
    Design = 0,
    Break = 1,
    Run = 2,
    Enc = 268435456,
    EncMask = -268435456
}

[MessagePackObject]
public record class VsDebuggerDebugModeInfo
{
    [Key(0)]
    public VsDebuggerDebugMode DebugMode { get; init; } = 0;
}

public interface IVsDebuggerEventsServiceHub
{
    ValueTask<int> OnModeChangeAsync(
        VsDebuggerDebugModeInfo info,
        CancellationToken cancellationToken);

    public static class Configuration
    {
        public const string ServiceName = "DebugAssistantExtension.Shared.ServiceHubs.IVsDebuggerEventsServiceHub";
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
