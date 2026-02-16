using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit { }
}
#endif

#if !NET5_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    public sealed class ExperimentalAttribute : Attribute
    {
        public ExperimentalAttribute(string diagnosticId)
            : base()
        { }
        public string DiagnosticId { get; } = "";
        public string? UrlFormat { get; set; } = null;
    }
}
#endif
