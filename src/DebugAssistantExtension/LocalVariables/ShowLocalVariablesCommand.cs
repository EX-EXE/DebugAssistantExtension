using DebugAssistantExtension.LocalVariables;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;


namespace DebugAssistantExtension.LocalVariables;

[VisualStudioContribution]
internal class ShowLocalVariablesCommand
    : Microsoft.VisualStudio.Extensibility.Commands.Command
{
    public ShowLocalVariablesCommand()
    {
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("%DebugAssistantExtension.ShowLocalVariablesCommand.DisplayName%")
    {
    };

    /// <inheritdoc />
    public override async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await base.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        await Extensibility.Shell().ShowToolWindowAsync<LocalVariablesToolWindow>(activate: true, cancellationToken);
    }
}
