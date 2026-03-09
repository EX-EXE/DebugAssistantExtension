using DebugAssistantExtension.VSExtensibility.Services;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;

namespace DebugAssistantExtension.VSExtensibility.MemoryVisualizers;

[VisualStudioContribution]
internal class MemoryVisualizerToolWindow : ToolWindow
{
    private readonly MemoryVisualizerWindow view;
    private readonly MemoryVisualizerViewModel viewModel;

    public MemoryVisualizerToolWindow(
        VisualStudioExtensibility extensibility,
        MemoryVisualizerService memoryVisualizerService)
        : base(extensibility)
    {
        Title = "Memory Visualizer";

        viewModel = new(memoryVisualizerService);
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
