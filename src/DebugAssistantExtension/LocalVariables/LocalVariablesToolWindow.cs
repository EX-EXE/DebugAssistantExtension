using DebugAssistantExtension.Services;
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

namespace DebugAssistantExtension.LocalVariables;

[VisualStudioContribution]
internal class LocalVariablesToolWindow : ToolWindow
{
    private readonly LocalVariablesWindow view;
    private readonly LocalVariablesViewModel viewModel;

    public LocalVariablesToolWindow(
        VisualStudioExtensibility extensibility,
        LocalVariablesService localVariablesService)
        : base(extensibility)
    {
        Title = "Local Variables";

        viewModel = new(localVariablesService);
        view = new(viewModel);
    }

    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
    };

    public override async Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        => view;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            view.Dispose();
            viewModel.Dispose();
        }
        base.Dispose(disposing);
    }
}
