using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Extensions;

public static class DebugPropertyExtension
{
    public static int MaxBufferSize = 256;

    public static bool TryGetProperty(
        this IDebugProperty2? debugProperty,
        enum_DEBUGPROP_INFO_FLAGS dwFields,
        uint dwRadix,
        uint dwTimeout,
        out DEBUG_PROPERTY_INFO outputDebugPropertyInfo)
    {
        outputDebugPropertyInfo = default;
        if (debugProperty == null)
        {
            return false;
        }
        var outputDebugPropertyInfos = new DEBUG_PROPERTY_INFO[1];
        var fetchPropertyInfoResult = debugProperty.GetPropertyInfo(
            dwFields,
            dwRadix,
            dwTimeout,
            null,
            0,
            outputDebugPropertyInfos);
        if (fetchPropertyInfoResult != 0 || outputDebugPropertyInfos.Length <= 0)
        {
            return false;
        }
        outputDebugPropertyInfo = outputDebugPropertyInfos[0];
        return true;
    }

    public static IEnumerable<DEBUG_PROPERTY_INFO> EnumerateChildren(
        this IDebugProperty2? debugProperty,
        enum_DEBUGPROP_INFO_FLAGS dwFields,
        uint dwRadix,
        Guid guidFilter,
        enum_DBG_ATTRIB_FLAGS dwAttribFilter,
        string? pszNameFilter,
        uint dwTimeout)
    {
        if (debugProperty == null)
        {
            yield break;
        }

        var enumChildrenResult = debugProperty.EnumChildren(
            dwFields,
            dwRadix,
            ref guidFilter,
            dwAttribFilter,
            pszNameFilter,
            dwTimeout,
            out var enumPropetyInfos);
        if (enumChildrenResult != 0 || enumPropetyInfos == null)
        {
            yield break;
        }


        var enumPropetyInfoCountResult = enumPropetyInfos.GetCount(out var enumPropetyInfoCount);
        if (enumPropetyInfoCountResult != 0 || enumPropetyInfoCount == 0)
        {
            yield break;
        }

        var remainingPropetyInfoNum = enumPropetyInfoCount;
        var fetchPropetyInfoNum = 0u;
        while (0 < remainingPropetyInfoNum)
        {
            var bufferSize = Math.Min(remainingPropetyInfoNum, MaxBufferSize);
            var fetchPropetyInfos = new DEBUG_PROPERTY_INFO[bufferSize];
            var fetchPropetyInfoResult = enumPropetyInfos.Next((uint)bufferSize, fetchPropetyInfos, out fetchPropetyInfoNum);
            if (fetchPropetyInfoResult != 0 || fetchPropetyInfos == null)
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

    public static bool TryGetMemoryBytes(
        this IDebugProperty2? debugProperty,
        long limitReadSize,
        byte? endByte,
        out byte[] outputBytes)
    {
        outputBytes = [];
        if (debugProperty == null)
        {
            return false;
        }

        var getMemoryBytesResult = debugProperty.GetMemoryBytes(out var debugMemoryBytes);
        if (getMemoryBytesResult != 0)
        {
            return false;
        }
        var getMemoryContextResult = debugProperty.GetMemoryContext(out var debugMemoryContext);
        if (getMemoryContextResult != 0)
        {
            return false;
        }

        int bufferSize = limitReadSize < MaxBufferSize ? (int)limitReadSize : MaxBufferSize;
        using var memoryBuffer = new MemoryBuffer<byte>(bufferSize);
        var memoryStream = new MemoryStream();
        var remainingSize = limitReadSize;
        var searchDebugMemoryContext = debugMemoryContext;
        var readedSize = 0u;
        var unreadSize = 0u;
        while (0 < remainingSize)
        {
            var readSize = Math.Min(remainingSize, memoryBuffer.Length);
            var readResult = debugMemoryBytes.ReadAt(searchDebugMemoryContext, (uint)readSize, memoryBuffer.Buffer, out readedSize, ref unreadSize);
            if (readResult != 0 || readedSize == 0)
            {
                break;
            }
            remainingSize -= readedSize;
            if (endByte != null)
            {
                // Search end byte
                var endByteIndex = memoryBuffer.Buffer.IndexOfByte(endByte.Value);
                if (0 <= endByteIndex)
                {
                    memoryStream.Write(memoryBuffer.Buffer, 0, endByteIndex);
                    break;
                }
            }
            // Write
            memoryStream.Write(memoryBuffer.Buffer, 0, (int)readedSize);

            // Next
            var moveNextResult = debugMemoryContext.Add(readedSize, out searchDebugMemoryContext);
            if (moveNextResult != 0)
            {
                break;
            }
        }
        outputBytes = memoryStream.ToArray();
        return true;
    }
}
