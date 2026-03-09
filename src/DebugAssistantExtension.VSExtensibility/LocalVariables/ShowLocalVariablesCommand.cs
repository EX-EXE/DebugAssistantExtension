using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;


namespace DebugAssistantExtension.VSExtensibility.LocalVariables;

[VisualStudioContribution]
internal class ShowLocalVariablesCommand
    : Command
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
