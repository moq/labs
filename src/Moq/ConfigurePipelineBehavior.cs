﻿using System.ComponentModel;
using Moq.Sdk;
using Avatars;

namespace Moq
{
    /// <summary>
    /// Configures the current invocation depending on the <see cref="MockBehavior"/> 
    /// specified for the mock.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ConfigurePipelineBehavior : IAvatarBehavior
    {
        /// <summary>
        /// Always applies to all invocations.
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => true;

        /// <summary>
        /// Configures the current invocation depending on the <see cref="MockBehavior"/> 
        /// specified for the mock.
        /// </summary>
        public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next)
        {
            var moq = invocation.Target.AsMoq();
            if (moq.Behavior != MockBehavior.Strict)
            {
                invocation.SkipBehaviors.Add(typeof(StrictMockBehavior));
            }

            // Ensure the Mock pipeline is always created for the matching setup
            // We need this to skip the StrictBehavior in the CallBaseBehavior
            if (SetupScope.IsActive)
            {
                invocation.Target.AsMock()
                    .GetPipeline(MockContext.CurrentSetup ?? CallContext.ThrowUnexpectedNull<IMockSetup>());
            }

            return next().Invoke(invocation, next);
        }
    }
}
