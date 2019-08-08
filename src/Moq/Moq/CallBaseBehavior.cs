﻿using Stunts;
using Moq.Sdk;
using System.Linq;
using System.ComponentModel;

namespace Moq
{
    /// <summary>
    /// A custom behavior to enable calls to the base member virtual implementation.
    /// See <see cref="CallBaseExtension"/> method calls.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CallBaseBehavior : IStuntBehavior
    {
        /// <inheritdoc />
        public bool AppliesTo(IMethodInvocation invocation) => true;

        /// <inheritdoc />
        public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next)
        {
            // Check if CallBase is configured at the Mock or Invocation level
            var shouldCallBase = invocation.Target.AsMoq().CallBase || invocation.Context.ContainsKey(nameof(IMoq.CallBase));

            if (shouldCallBase)
            {
                // Skip the default value to force the base target member is executed
                invocation.SkipBehaviors.Add(typeof(DefaultValueBehavior));

                // If there is a matching setup for the current invocation, skip the strict 
                // behavior because CallBase should be called instead
                if (invocation.Target.AsMock().Behaviors.OfType<IMockBehaviorPipeline>().Any(x => x.AppliesTo(invocation)))
                    invocation.SkipBehaviors.Add(typeof(StrictMockBehavior));
            }

            return next().Invoke(invocation, next);
        }
    }
}