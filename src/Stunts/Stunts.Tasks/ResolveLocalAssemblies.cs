using Microsoft.Build.Utilities;

namespace Stunts.Tasks
{
    public class ResolveLocalAssemblies : Task
    {
        public override bool Execute()
        {
            AssemblyResolver.Init();
            return true;
        }
    }
}