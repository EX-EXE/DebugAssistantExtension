using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Extensions;

internal static class DebugThreadExtension
{
    public static int MaxBufferSize = 256;

    public static IEnumerable<FRAMEINFO> EnumerateFrameInfo(
        this IDebugThread2? threadInfo,
        enum_FRAMEINFO_FLAGS dwFieldSpec,
        uint nRadix)
    {
        if(threadInfo == null)
        {
            yield break;
        }

        var enumFrameInfoResult = threadInfo.EnumFrameInfo(
            dwFieldSpec,
            nRadix,
            out var enumFrameInfo);
        if (enumFrameInfoResult != 0 || enumFrameInfo == null)
        {
            yield break;
        }

        var enumFrameInfoCountResult = enumFrameInfo.GetCount(out var enumFrameInfoCount);
        if (enumFrameInfoCountResult != 0 || enumFrameInfoCount == 0)
        {
            yield break;
        }

        var remainingFrameInfoNum = enumFrameInfoCount;
        var fetchFrameInfoNum = 0u;
        while (0 < remainingFrameInfoNum)
        {
            var bufferSize = Math.Min(remainingFrameInfoNum, MaxBufferSize);
            var fetchFrameInfos = new FRAMEINFO[bufferSize];
            var fetchFrameInfoResult = enumFrameInfo.Next((uint)bufferSize, fetchFrameInfos, ref fetchFrameInfoNum);
            if (fetchFrameInfoResult != 0)
            {
                break;
            }
            remainingFrameInfoNum -= fetchFrameInfoNum;
            foreach (var frameInfo in fetchFrameInfos.Take((int)fetchFrameInfoNum))
            {
                yield return frameInfo;
            }
        }
    }
}
