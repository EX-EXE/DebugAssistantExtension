using DebugAssistantExtension.Services;
using ObservableCollections;
using R3;
using System.Runtime.Serialization;

namespace DebugAssistantExtension.LocalVariables;


[DataContract]
internal class LocalVariablesViewModel
    : IDisposable
{
    [DataMember]
    public NotifyCollectionChangedSynchronizedViewList<LocalVariableInfo> ItemsView { get; private set; }


    private readonly IDisposable disposable;
    private bool disposedValue;

    public LocalVariablesViewModel(
        LocalVariablesService localVariablesService)
    {
        var disposableBuilder = new DisposableBuilder();
        ItemsView = localVariablesService.Items.ToNotifyCollectionChangedSlim();
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
