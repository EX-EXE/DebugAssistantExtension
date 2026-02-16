using DebugAssistantExtension.Extensions;
using EnvDTE;
using EnvDTE80;
using EnvDTE90a;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using R3;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DebugAssistantExtension.Services;

#pragma warning disable VSEXTPREVIEW_SETTINGS // The settings API is currently in preview and marked as experimental

[DataContract]
public class LocalVariableInfo
{
    [DataMember]
    public string Name { get; set; } = string.Empty;
    [DataMember]
    public string Value { get; set; } = string.Empty;
    [DataMember]
    public string Type { get; set; } = string.Empty;
    [DataMember]
    public string Size { get; set; } = string.Empty;
    [DataMember]
    public string IndirectSize { get; set; } = string.Empty;

    [DataMember]
    public string MemoryHex { get; set; } = string.Empty;
    [DataMember]
    public string MemoryAscii { get; set; } = string.Empty;

}

internal class LocalVariablesService
    : IAsyncDisposable
    , IDebugEventCallback2
    , IVsDebuggerEvents
{
    private readonly TraceSource logger;
    private readonly AsyncServiceProviderInjection<DTE, DTE2> dteServiceProvider;
    private readonly Settings.LocalVariablesCategoryObserver localVariablesCategoryObserver;

    private DTE2? dteInstance = null;

    public ReactiveProperty<bool> Enable { get; private set; } = new(true);
    public ReactiveProperty<int> MaxDepth { get; private set; } = new(16);
    public ReactiveProperty<int> MaxReadMemorySize { get; private set; } = new(256);
    public ObservableCollections.ObservableList<LocalVariableInfo> Items { get; private set; }

    private uint? debuggerEventsCookie = null;



    public LocalVariablesService(
        TraceSource logger,
        AsyncServiceProviderInjection<DTE, DTE2> dteServiceProvider,
        Settings.LocalVariablesCategoryObserver localVariablesCategoryObserver)
    {
        this.logger = logger;
        this.dteServiceProvider = dteServiceProvider;
        this.localVariablesCategoryObserver = localVariablesCategoryObserver;

        localVariablesCategoryObserver.Changed += async (snapShot) =>
            {

                Enable.Value = snapShot.LocalVariablesEnable.Value;
                MaxDepth.Value = snapShot.LocalVariablesMaxDepth.Value;
                MaxReadMemorySize.Value = snapShot.LocalVariablesMaxReadMemorySize.Value;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                UpdateDebugInfo();
            };
        Items = new();
    }

    public async ValueTask DisposeAsync()
    {
        var dteInstance = await dteServiceProvider.GetServiceAsync();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var vsDebugger = (IVsDebugger?)Package.GetGlobalService(typeof(SVsShellDebugger));
        if (vsDebugger != null)
        {
            vsDebugger.UnadviseDebugEventCallback(this);
            if (debuggerEventsCookie.HasValue)
            {
                vsDebugger.UnadviseDebuggerEvents(debuggerEventsCookie.Value);
            }
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        dteInstance = await dteServiceProvider.GetServiceAsync();

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var vsDebugger = (IVsDebugger?)Package.GetGlobalService(typeof(SVsShellDebugger));
        if (vsDebugger != null)
        {
            vsDebugger.AdviseDebugEventCallback(this);
            vsDebugger.AdviseDebuggerEvents(this, out var pdwCookie);
            debuggerEventsCookie = pdwCookie;
        }
    }

    public class DebugInfo
    {
        public IDebugEngine2? Engine { get; set; } = null;
        public IDebugProcess2? Process { get; set; } = null;
        public IDebugProgram2? Program { get; set; } = null;
        public IDebugThread2? Thread { get; set; } = null;
        public IDebugEvent2? Event { get; set; } = null;
        public Guid EventGuid { get; set; } = new System.Guid();
        public uint EventAttributes { get; set; } = 0;

        public uint StackFrameDepth = 0;
    }
    private DebugInfo? lastDebugInfo = null;

    /// <inheritdoc>/>
    public int Event(
        IDebugEngine2 pEngine,
        IDebugProcess2 pProcess,
        IDebugProgram2 pProgram,
        IDebugThread2 pThread,
        IDebugEvent2 pEvent,
        ref Guid riidEvent,
        uint dwAttrib)
    {
        if (riidEvent == GuidConstants.DebugChangeContext)
        {
            if (pProcess != null &&
                pProgram != null &&
                pThread != null &&
                pEvent != null)
            {
                this.logger.TraceInformation($"Debug event: {riidEvent}, Attributes: {dwAttrib}");
                uint GetCurrentStackFrameDepth()
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    if (dteInstance != null)
                    {
                        var currentStackFrame = dteInstance.Debugger.CurrentStackFrame;
                        if (currentStackFrame is StackFrame2 stackFrame2)
                        {
                            return 0 < stackFrame2.Depth
                                ? stackFrame2.Depth - 1
                                : 0;
                        }
                    }
                    return 0;
                }
                lastDebugInfo = new DebugInfo()
                {
                    Engine = pEngine,
                    Process = pProcess,
                    Program = pProgram,
                    Thread = pThread,
                    Event = pEvent,
                    EventGuid = riidEvent,
                    EventAttributes = dwAttrib,

                    StackFrameDepth = GetCurrentStackFrameDepth(),
                };
                UpdateDebugInfo();
            }
        }

        return 0;
    }

    /// <inheritdoc>/>
    public int OnModeChange(DBGMODE dbgmodeNew)
    {
        if (dbgmodeNew != DBGMODE.DBGMODE_Break)
        {
            ClearUpdateInfo();
        }
        return 0;
    }

    void ClearUpdateInfo()
    {
        lastDebugInfo = null;
        Items.Clear();
    }

    void UpdateDebugInfo()
    {
        Items.Clear();
        if (dteInstance == null || lastDebugInfo == null)
        {
            return;
        }
        try
        {
            FRAMEINFO? currentFrameInfo = lastDebugInfo.Thread.EnumerateFrameInfo(
                enum_FRAMEINFO_FLAGS.FIF_FRAME,
                10)
                .Skip((int)lastDebugInfo.StackFrameDepth)
                .FirstOrDefault();
            if (currentFrameInfo != null &&
                currentFrameInfo.TryGetExpressionContext(out var debugExpressionContext))
            {
                // C++ Only
                string languageString = "";
                Guid languageGuid = new Guid();
                if (currentFrameInfo.Value.m_pFrame == null || 
                    currentFrameInfo.Value.m_pFrame.GetLanguageInfo(ref languageString, ref languageGuid) != 0 &&
                    languageGuid != GuidConstants.CppLanguage)
                {
                    return;
                }

                foreach (var propertyInfo in currentFrameInfo.EnumeratePropertiesWithChildren(
                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
                    10,
                    GuidConstants.FilterLocals,
                    0,
                    MaxDepth.Value))
                {
                    bool TryGetEvaluatePropertySync(
                        IDebugExpressionContext2? debugExpressionContext,
                        string code,
                        out DEBUG_PROPERTY_INFO outputDebugPropertyInfo)
                        => debugExpressionContext.TryGetEvaluatePropertySync(
                                code,
                                enum_PARSEFLAGS.PARSE_EXPRESSION,
                                10,
                                enum_EVALFLAGS.EVAL_NOSIDEEFFECTS,
                                0,
                                enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
                                out outputDebugPropertyInfo);
                    bool TryGetEvaluateMemoryBytesSync(
                        IDebugExpressionContext2? debugExpressionContext,
                        string code,
                        long limitReadSize,
                        byte? endByte,
                        out byte[] outputBytes)
                        => debugExpressionContext.TryGetEvaluateMemoryBytesSync(
                                code,
                                enum_PARSEFLAGS.PARSE_EXPRESSION,
                                10,
                                enum_EVALFLAGS.EVAL_NOSIDEEFFECTS,
                                0,
                                enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL,
                                limitReadSize,
                                endByte,
                                out outputBytes);


                    var size = "";
                    if (TryGetEvaluatePropertySync(
                        debugExpressionContext,
                        $"sizeof({propertyInfo.bstrFullName})",
                        out var debugPropertyInfoSize))
                    {
                        size = debugPropertyInfoSize.bstrValue;
                    }

                    var indirectSize = "";
                    var memoryHex = "";
                    var memoryAscii = "";
                    if (propertyInfo.bstrType.Contains("*"))
                    {
                        if (TryGetEvaluatePropertySync(
                            debugExpressionContext,
                            $"sizeof(*{propertyInfo.bstrFullName})",
                            out var outputIndirectSize))
                        {
                            indirectSize = outputIndirectSize.bstrValue;
                        }

                        if (propertyInfo.bstrType.Contains("char"))
                        {
                            // const char*
                            if (TryGetEvaluateMemoryBytesSync(
                                debugExpressionContext,
                                propertyInfo.bstrFullName,
                                MaxReadMemorySize.Value,
                                0x0,
                                out var stringBytes))
                            {
                                memoryHex = stringBytes.ToHexString();
                                memoryAscii = stringBytes.ToAciiString();
                            }
                        }
                        else if (int.TryParse(indirectSize, out var indirectSizeNum))
                        {
                            // Pointer Type
                            var requestReadSize = Math.Min(indirectSizeNum, MaxReadMemorySize.Value);
                            if (TryGetEvaluateMemoryBytesSync(
                                debugExpressionContext,
                                propertyInfo.bstrFullName,
                                requestReadSize,
                                null,
                                out var stringBytes))
                            {
                                memoryHex = stringBytes.ToHexString();
                                memoryAscii = stringBytes.ToAciiString();
                            }
                        }
                    }

                    Items.Add(new LocalVariableInfo()
                    {
                        Name = propertyInfo.bstrFullName,
                        Value = propertyInfo.bstrValue,
                        Type = propertyInfo.bstrType,

                        Size = size,
                        IndirectSize = indirectSize,

                        MemoryHex = memoryHex,
                        MemoryAscii = memoryAscii,
                    });
                }
            }
        }
        catch (Exception ex)
        {
            logger.TraceInformation($"Failed to update local variables info: {ex}");
        }
    }

}