using DebugAssistantExtension.Shared.ServiceHubs;
using DebugAssistantExtension.Shared.Services;
using Microsoft.VisualStudio.Extensibility;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DebugAssistantExtension.VSExtensibility.Services;

internal class MemoryVisualizerService
{
    public ReactiveProperty<DebugPropertyMemoryInfo?> LatestMemoryInfo = new(null);
}

