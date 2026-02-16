using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Extensions;

internal static class FrameInfoExtension
{
    public static int MaxBufferSize = 256;

    public static IEnumerable<DEBUG_PROPERTY_INFO> EnumerateProperties(
        this FRAMEINFO? frameInfo,
        enum_DEBUGPROP_INFO_FLAGS dwFields,
        uint nRadix,
        Guid guidFilter,
        uint dwTimeout)
    {
        if (frameInfo == null || frameInfo.Value.m_pFrame == null)
        {
            yield break;
        }
        var enumPropertiesResult = frameInfo.Value.m_pFrame.EnumProperties(
            dwFields,
            nRadix,
            ref guidFilter,
            dwTimeout,
            out var enumPropetyInfoNum,
            out var enumPropetyInfos);
        if (enumPropertiesResult != 0 || enumPropetyInfos == null)
        {
            yield break;
        }

        var remainingPropetyInfoNum = enumPropetyInfoNum;
        while (0 < remainingPropetyInfoNum)
        {
            var bufferSize = Math.Min(remainingPropetyInfoNum, MaxBufferSize);
            var fetchPropetyInfos = new DEBUG_PROPERTY_INFO[bufferSize];
            var fetchPropetyInfoResult = enumPropetyInfos.Next((uint)bufferSize, fetchPropetyInfos, out var fetchPropetyInfoNum);
            if (fetchPropetyInfoResult != 0)
            {
                break;
            }
            remainingPropetyInfoNum -= fetchPropetyInfoNum;
            foreach (var fetchPropetyInfo in fetchPropetyInfos.Take((int)fetchPropetyInfoNum))
            {
                yield return fetchPropetyInfo;
            }
        }
    }

    public static bool TryGetExpressionContext(
        this FRAMEINFO? frameInfo,
        out IDebugExpressionContext2? debugExpressionContext)
    {
        debugExpressionContext = null;
        if (frameInfo == null || frameInfo.Value.m_pFrame == null)
        {
            return false;
        }
        var getExpressionContextResult = frameInfo.Value.m_pFrame.GetExpressionContext(out debugExpressionContext);
        return (getExpressionContextResult == 0 && debugExpressionContext != null);
    }


}