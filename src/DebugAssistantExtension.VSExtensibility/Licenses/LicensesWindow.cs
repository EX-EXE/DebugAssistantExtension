using Microsoft.VisualStudio.Extensibility.UI;

namespace DebugAssistantExtension.VSExtensibility.Licenses;

internal class LicensesWindow
    : RemoteUserControl
{
    public LicensesWindow()
        : base(dataContext: null)
    {
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
