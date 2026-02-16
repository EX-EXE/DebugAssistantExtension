using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugAssistantExtension.Licenses;

internal class LicensesWindow
    : RemoteUserControl
{
    public LicensesWindow()
        : base(dataContext: null)
    {
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
