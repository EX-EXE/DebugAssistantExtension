using Microsoft.VisualStudio.Extensibility.UI;

namespace DebugAssistantExtension.VSExtensibility.LocalVariables;


internal class LocalVariablesWindow
    : RemoteUserControl
{
    public LocalVariablesWindow(
        object? dataContext)
        : base(dataContext: dataContext)
    {
    }
}
