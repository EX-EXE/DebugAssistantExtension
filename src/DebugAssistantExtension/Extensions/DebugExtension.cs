using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Extensions;

public static class DebugExtension
{
    public static IEnumerable<DEBUG_PROPERTY_INFO> EnumeratePropertiesWithChildren(
        this FRAMEINFO? frameInfo,
        enum_DEBUGPROP_INFO_FLAGS dwFields,
        uint nRadix,
        Guid guidFilter,
        uint dwTimeout,
        int maxDepth)
    {
        static IEnumerable<DEBUG_PROPERTY_INFO> InnerEnumeratePropertiesWithChildren(
            DEBUG_PROPERTY_INFO? debugPropertyInfo,
            enum_DEBUGPROP_INFO_FLAGS dwFields,
            uint nRadix,
            Guid guidFilter,
            uint dwTimeout,
            int currentDepth,
            int maxDepth)
        {
            if (debugPropertyInfo == null || debugPropertyInfo.Value.pProperty == null)
            {
                yield break;
            }
            if (maxDepth <= currentDepth)
            {
                yield break;
            }

            var baseClassPropertyInfos = new List<DEBUG_PROPERTY_INFO>();
            var innerClassPropertyInfos = new List<DEBUG_PROPERTY_INFO>();

            foreach (var childPropertyInfo in debugPropertyInfo.Value.pProperty.EnumerateChildren(
                dwFields,
                nRadix,
                guidFilter,
                enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_ALL,
                null,
                dwTimeout))
            {
                if ((childPropertyInfo.dwAttrib & enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_BASECLASS) == enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_BASECLASS)
                {
                    baseClassPropertyInfos.Add(childPropertyInfo);
                }
                else if ((childPropertyInfo.dwAttrib & enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_CLASS) == enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_CLASS)
                {
                    innerClassPropertyInfos.Add(childPropertyInfo);
                }
                else
                {
                    yield return childPropertyInfo;
                }
            }
            foreach(var innerClassPropertyInfo in innerClassPropertyInfos)
            {
                foreach (var childPropertyInfo in InnerEnumeratePropertiesWithChildren(
                                innerClassPropertyInfo,
                                dwFields,
                                nRadix,
                                guidFilter,
                                dwTimeout,
                                currentDepth + 1,
                                maxDepth))
                {
                    yield return childPropertyInfo;
                }
            }
            foreach (var baseClassPropertyInfo in baseClassPropertyInfos)
            {
                foreach (var childPropertyInfo in InnerEnumeratePropertiesWithChildren(
                                baseClassPropertyInfo,
                                dwFields,
                                nRadix,
                                guidFilter,
                                dwTimeout,
                                currentDepth,
                                maxDepth))
                {
                    yield return childPropertyInfo;
                }
            }
        }


        foreach (var rootPropertyInfo in frameInfo.EnumerateProperties(
            dwFields,
            nRadix,
            guidFilter,
            dwTimeout))
        {
            yield return rootPropertyInfo;
            foreach (var childrenPropertyInfo in InnerEnumeratePropertiesWithChildren(
                rootPropertyInfo,
                dwFields,
                nRadix,
                guidFilter,
                dwTimeout,
                0,
                maxDepth))
            {
                yield return childrenPropertyInfo;
            }
        }
    }



    public static bool TryGetEvaluatePropertySync(
        this IDebugExpressionContext2? debugExpressionContext,
        string pszCode,
        enum_PARSEFLAGS dwParseFlags,
        uint nRadix,
        enum_EVALFLAGS dwEvalFlags,
        uint dwTimeout,
        enum_DEBUGPROP_INFO_FLAGS dwDebugPropInfoFields,
        out DEBUG_PROPERTY_INFO outputDebugPropertyInfo)
    {
        outputDebugPropertyInfo = default;
        if (debugExpressionContext == null)
        {
            return false;
        }
        if (debugExpressionContext.TryGetEvaluateSync(
            pszCode,
            dwParseFlags,
            nRadix,
            dwEvalFlags,
            dwTimeout,
            out var debugProperty))
        {
            if (debugProperty.TryGetProperty(
                dwDebugPropInfoFields,
                nRadix,
                dwTimeout,
                out var debugPropertyInfo))
            {
                outputDebugPropertyInfo = debugPropertyInfo;
                return true;
            }
        }
        return false;
    }


    public static bool TryGetEvaluateMemoryBytesSync(
        this IDebugExpressionContext2? debugExpressionContext,
        string pszCode,
        enum_PARSEFLAGS dwParseFlags,
        uint nRadix,
        enum_EVALFLAGS dwEvalFlags,
        uint dwTimeout,
        enum_DEBUGPROP_INFO_FLAGS dwDebugPropInfoFields,
        long limitReadSize,
        byte? endByte,
        out byte[] outputBytes)
    {
        outputBytes = [];
        if (debugExpressionContext == null)
        {
            return false;
        }
        if (debugExpressionContext.TryGetEvaluateSync(
            pszCode,
            dwParseFlags,
            nRadix,
            dwEvalFlags,
            dwTimeout,
            out var debugProperty))
        {
            if (debugProperty.TryGetMemoryBytes(
                limitReadSize,
                endByte,
                out outputBytes))
            {
                return true;
            }
        }
        return false;
    }

}
