using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;

namespace Stunts.Tasks
{
    interface IWorkspace
    {
        Project GetOrAddProject(IBuildEngine buildEngine, string projectPath, CancellationToken cancellation);
    }
}
