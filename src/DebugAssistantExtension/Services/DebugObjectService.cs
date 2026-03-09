using DebugAssistantExtension.Extensions;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Services;

internal class DebugObjectService 
{
    public ConcurrentDictionary<Guid, IDebugProperty3> IDebugProperty3Dictionary { get; private set; } = new();
    public ConcurrentDictionary<Guid, IDebugEngine2> IDebugEngine2Dictionary { get; private set; } = new();
    public ConcurrentDictionary<Guid, IDebugProcess2> IDebugProcess2Dictionary { get; private set; } = new();
    public ConcurrentDictionary<Guid, IDebugProgram2> IDebugProgram2Dictionary { get; private set; } = new();
    public ConcurrentDictionary<Guid, IDebugThread2> IDebugThread2Dictionary { get; private set; } = new();
    public ConcurrentDictionary<Guid, IDebugEvent2> IDebugEvent2Dictionary { get; private set; } = new();


    public ConcurrentDictionary<Guid, FRAMEINFO> FrameInfoDictionary { get; private set; } = new();
    public ConcurrentDictionary<Guid, DEBUG_PROPERTY_INFO> DebugPropertyInfoDictionary { get; private set; } = new();
    public ConcurrentDictionary<Guid, IDebugExpressionContext2> IDebugExpressionContext2Dictionary { get; private set; } = new();

    public ConcurrentDictionary<Guid, IDebugProperty2> DebugProperty2Dictionary { get; private set; } = new();

}
