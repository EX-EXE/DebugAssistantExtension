using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DebugAssistantExtension.Extensions;

internal class MemoryBuffer<T> : IDisposable
{
    public T[] Buffer { get; private set; }
    public int Length => Buffer.Length;

    public MemoryBuffer(int size)
    {
        Buffer = ArrayPool<T>.Shared.Rent(size);
    }
    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(Buffer);
    }
}
