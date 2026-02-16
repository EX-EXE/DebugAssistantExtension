using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Licenses;

[VisualStudioContribution]
internal class LicensesToolWindow : ToolWindow
{
    private readonly LicensesWindow view;

    public LicensesToolWindow(
        VisualStudioExtensibility extensibility)
        : base(extensibility)
    {
        Title = "License";
        view = new();
    }

    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
    };

    public override async Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        => view;

    public override async Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await view.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            view.Dispose();
        }
        base.Dispose(disposing);
    }
}
