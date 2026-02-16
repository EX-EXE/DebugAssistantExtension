using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension;

#pragma warning disable VSEXTPREVIEW_SETTINGS // The settings API is currently in preview and marked as experimental

internal static class ExtensionSettingDefinitions
{
    [VisualStudioContribution]
    private static SettingCategory DebugAssistantCategory { get; } = new("debugAssistant", "%DebugAssistantExtension.DebugAssistant.DisplayName%")
    {
        GenerateObserverClass = true,
    };

    [VisualStudioContribution]
    private static SettingCategory LocalVariablesCategory { get; } = new("localVariables", "%DebugAssistantExtension.LocalVariable.DisplayName%", DebugAssistantCategory)
    {
        GenerateObserverClass = true,
    };

    [VisualStudioContribution]
    private static Setting.Boolean LocalVariablesEnable { get; } = new("localVariablesEnable", "%DebugAssistantExtension.Enable.DisplayName%", LocalVariablesCategory, defaultValue: true);

    [VisualStudioContribution]
    private static Setting.Integer LocalVariablesMaxDepth { get; } = new("localVariablesMaxDepth", "%DebugAssistantExtension.MaxDepth.DisplayName%", LocalVariablesCategory, defaultValue: 16);

    [VisualStudioContribution]
    private static Setting.Integer LocalVariablesMaxReadMemorySize { get; } = new("localVariablesMaxReadMemorySize", "%DebugAssistantExtension.MaxReadMemorySize.DisplayName%", LocalVariablesCategory, defaultValue: 512);

}