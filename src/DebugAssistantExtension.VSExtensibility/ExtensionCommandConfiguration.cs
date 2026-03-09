using DebugAssistantExtension.VSExtensibility.Licenses;
using DebugAssistantExtension.VSExtensibility.LocalVariables;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace DebugAssistantExtension.VSExtensibility;

internal static class ExtensionCommandConfiguration
{
    [VisualStudioContribution]
    public static MenuConfiguration CommentRemoverMenu
        => new("%DebugAssistantExtension.Menu.DisplayName%")
        {
            Placements =
            [
                CommandPlacement.KnownPlacements.ExtensionsMenu.WithPriority(0x01),
            ],
            Children =
            [
                MenuChild.Command<ShowLocalVariablesCommand>(),
                MenuChild.Separator,
                MenuChild.Command<ShowLicensesCommand>(),
            ],
        };
}
