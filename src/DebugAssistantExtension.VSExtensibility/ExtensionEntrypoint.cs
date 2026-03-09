
using DebugAssistantExtension.Shared.ServiceHubs;
using DebugAssistantExtension.VSExtensibility.ServiceHubs;
using DebugAssistantExtension.VSExtensibility.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace DebugAssistantExtension.VSExtensibility;

/// <summary>
/// Extension entrypoint for the VisualStudio.Extensibility extension.
/// </summary>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <inheritdoc />
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
                id: "DebugAssistantExtension.30016d8f-6182-46d0-a972-d26dec3a7710",
                version: this.ExtensionAssemblyVersion,
                publisherName: "Microsoft",
                displayName: "Extension with Traditional Components",
                description: "Shows how to implement an out-of-process extension that packages traditional content."),

        LoadedWhen = ActivationConstraint.Or(
            ActivationConstraint.SolutionState(SolutionState.Exists),
            ActivationConstraint.SolutionState(SolutionState.NoSolution)
            //ActivationConstraint.UIContext(new Guid(UIContextGuids80.Debugging))
            ),
    };
    //public override ExtensionConfiguration ExtensionConfiguration => new()
    //{
    //    //RequiresInProcessHosting = false,
    //    //LoadedWhen = ActivationConstraint.Or(
    //    //    ActivationConstraint.SolutionState(SolutionState.Exists),
    //    //    ActivationConstraint.SolutionState(SolutionState.NoSolution)
    //    //    //ActivationConstraint.UIContext(new Guid(UIContextGuids80.Debugging))
    //    //    ),
    //};

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
        serviceCollection.AddSingleton<LocalVariablesService>();
        serviceCollection.AddSingleton<MemoryVisualizerService>();

        serviceCollection.ProfferBrokeredService(
            VsCppDebugServiceHub.BrokeredServiceConfiguration,
            IVsCppDebugServiceHub.Configuration.ServiceDescriptor);
        serviceCollection.ProfferBrokeredService(
            VsDebuggerEventsServiceHub.BrokeredServiceConfiguration,
            IVsDebuggerEventsServiceHub.Configuration.ServiceDescriptor);
        serviceCollection.ProfferBrokeredService(
            DebugEventCallback2ServiceHub.BrokeredServiceConfiguration,
            IDebugEventCallback2ServiceHub.Configuration.ServiceDescriptor);
    }

    protected override async Task OnInitializedAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        //AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        //{
        //    if (args.Name.StartsWith("System.ComponentModel.Annotations"))
        //    {
        //        var assemblyDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //        var loadDll = System.IO.Path.Combine(assemblyDirectory, "System.ComponentModel.Annotations.dll");
        //        if (System.IO.File.Exists(loadDll))
        //        {
        //            return Assembly.LoadFrom(loadDll);
        //        }
        //    }
        //    return null;
        //};
        await base.OnInitializedAsync(extensibility, cancellationToken);
        var localVariablesService = ServiceProvider.GetRequiredService<LocalVariablesService>();
        await localVariablesService.InitializeAsync(cancellationToken);

    }
}
