using Microsoft.VisualStudio.Extensibility.UI;

namespace DebugAssistantExtension.VSExtensibility.MemoryVisualizers;


internal class MemoryVisualizerWindow
    : RemoteUserControl
{
    public MemoryVisualizerWindow(
        object? dataContext)
        : base(dataContext: dataContext)
    {
    }
}
