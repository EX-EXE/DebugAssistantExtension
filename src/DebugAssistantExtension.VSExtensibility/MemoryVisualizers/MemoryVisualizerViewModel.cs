using DebugAssistantExtension.VSExtensibility.Services;
using ObservableCollections;
using R3;
using System.Runtime.Serialization;

namespace DebugAssistantExtension.VSExtensibility.MemoryVisualizers;


[DataContract]
internal record class MemoryVisualizerInfo
{
    [DataMember]
    public string Hex { get; init; } = "";
    [DataMember]
    public string Ascii { get; init; } = "";

}

[DataContract]
internal class MemoryVisualizerViewModel
    : IDisposable
{
    [DataMember]
    public NotifyCollectionChangedSynchronizedViewList<MemoryVisualizerInfo> ItemsView { get; private set; }

    private readonly ObservableList<MemoryVisualizerInfo> items = new();

    private readonly IDisposable disposable;
    private bool disposedValue;

    public MemoryVisualizerViewModel(
        MemoryVisualizerService memoryVisualizerService)
    {
        var disposableBuilder = new DisposableBuilder();

        ItemsView = items
            .ToNotifyCollectionChangedSlim()
            .AddTo(ref disposableBuilder);

        memoryVisualizerService.LatestMemoryInfo.Subscribe(x =>
        {
            items.Clear();
            if (x == null)
            {
                return;
            }
            foreach (var memory in x.Memory.Chunk(16))
            {
                items.Add(new MemoryVisualizerInfo
                {
                    Hex = string.Join(" ", memory.Select(b => b.ToString("X2"))),
                    Ascii = string.Concat(memory.Select(b => b >= 32 && b <= 126 ? (char)b : '.'))
                });
            }

        }).AddTo(ref disposableBuilder);

        disposable = disposableBuilder.Build();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                disposable.Dispose();
            }
            disposedValue = true;
        }
    }
}
