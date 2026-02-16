using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace DebugAssistantExtension.Licenses;

/// <summary>
/// Command1 handler.
/// </summary>
[VisualStudioContribution]
internal class ShowLicensesCommand
    : Microsoft.VisualStudio.Extensibility.Commands.Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowLicensesCommand"/> class.
    /// </summary>
    /// <param name="traceSource">Trace source instance to utilize.</param>
    public ShowLicensesCommand()
    {
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("%DebugAssistantExtension.ShowLicenseCommand.DisplayName%")
    {
    };

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        await Extensibility.Shell().ShowToolWindowAsync<LicensesToolWindow>(activate: true, cancellationToken);
    }
}
