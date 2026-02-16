using DebugAssistantExtension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;
using Microsoft.VisualStudio.Shell.Interop;

namespace DebugAssistantExtension;

/// <summary>
/// Extension entrypoint for the VisualStudio.Extensibility extension.
/// </summary>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <inheritdoc />
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        RequiresInProcessHosting = true,
        LoadedWhen = ActivationConstraint.Or(
            ActivationConstraint.SolutionState(SolutionState.Exists),
            ActivationConstraint.SolutionState(SolutionState.NoSolution),
            ActivationConstraint.UIContext(new Guid(UIContextGuids80.Debugging))),
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
        serviceCollection.AddSettingsObservers();
        serviceCollection.AddSingleton<LocalVariablesService>();
    }

    protected override async Task OnInitializedAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        await base.OnInitializedAsync(extensibility, cancellationToken);
        var localVariablesService = ServiceProvider.GetRequiredService<LocalVariablesService>();
        await localVariablesService.InitializeAsync(cancellationToken);

    }
}
