using System.Diagnostics;

namespace Stunts
{
    /// <summary>
    /// A general purpose <see cref="IStuntBehavior"/> that invokes the provided 
    /// delegates for the <see cref="IStuntBehavior.Execute(IMethodInvocation, GetNextBehavior)"/> 
    /// (via a <see cref="ExecuteDelegate"/>) and 
    /// <see cref="IStuntBehavior.AppliesTo(IMethodInvocation)"/> (via a <see cref="AppliesToDelegate"/>) 
    /// interface methods.
    /// </summary>
    public class DelegateStuntBehavior : IStuntBehavior
    {
        AppliesToDelegate appliesTo;
        ExecuteDelegate behavior;
        string name;

        /// <summary>
        /// Creates the behavior from the given delegates.
        /// </summary>
        public DelegateStuntBehavior(ExecuteDelegate behavior, AppliesToDelegate appliesTo = null, string name = null)
        {
            this.behavior = behavior;
            this.appliesTo = appliesTo ?? new AppliesToDelegate(invocation => true);
            this.name = name;
        }

        /// <summary>
        /// Executes the provided <see cref="AppliesToDelegate"/> provided in the constructor.
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => appliesTo(invocation);

        /// <summary>
        /// Executes the provided <see cref="ExecuteDelegate"/> provided in the constructor.
        /// </summary>
        public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next) =>
            behavior(invocation, next);

        /// <summary>
        /// Gets a friendly representation of the object.
        /// </summary>
        /// <devdoc>
        /// We don't want to optimize code coverage for this since it's a debugger aid only. 
        /// Annotating this method with DebuggerNonUserCode achieves that.
        /// No actual behavior depends on these strings.
        /// </devdoc>
        [DebuggerNonUserCode]
        public override string ToString() => name ?? "<unnamed>";
    }
}
