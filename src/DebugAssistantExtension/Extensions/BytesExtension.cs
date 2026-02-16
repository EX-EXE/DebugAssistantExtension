using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Extensions;

public static class BytesExtension
{
    public static int IndexOfByte(this byte[] bytes, byte value)
    {
        if (bytes == null)
        {
            return -1;
        }
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == value)
            {
                return i;
            }
        }
        return -1;
    }

    public static string ToHexString(this byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return string.Empty;
        }
        var hexString = BitConverter.ToString(bytes).Replace("-", " ");
        return hexString;
    }

    public static string ToAciiString(this byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return string.Empty;
        }
        var asciiChars = bytes.Select(b => (b >= 32 && b <= 126) ? (char)b : '.');
        return new string(asciiChars.ToArray());
    }
}
