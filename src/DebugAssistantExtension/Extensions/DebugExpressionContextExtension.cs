using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DebugAssistantExtension.Extensions;

internal static class DebugExpressionContextExtension
{
    public static int MaxBufferSize = 256;

    public static bool TryGetEvaluateSync(
        this IDebugExpressionContext2? debugExpressionContext,
        string pszCode,
        enum_PARSEFLAGS dwParseFlags,
        uint nRadix,
        enum_EVALFLAGS dwEvalFlags,
        uint dwTimeout,
        out IDebugProperty2 outputDebugProperty)
    {
        outputDebugProperty = null!;
        if (debugExpressionContext == null)
        {
            return false;
        }
        var parseResult = debugExpressionContext.ParseText(
            pszCode,
            dwParseFlags,
            nRadix,
            out var expression,
            out _,
            out _);
        if (parseResult != 0 || expression == null)
        {
            return false;
        }

        var evaluateResult = expression.EvaluateSync(
            dwEvalFlags,
            0,
            null,
            out outputDebugProperty);
        return (evaluateResult == 0 && outputDebugProperty != null);
    }
}