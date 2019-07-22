﻿using System;
using Stunts;
using System.Diagnostics;

namespace Moq.Sdk
{
    /// <summary>
    /// Records invocations performed on the mock, as long as 
    /// <see cref="SetupScope.IsActive"/> is <see langword="false" />.
    /// </summary>
    public class MockRecordingBehavior : IStuntBehavior
    {
        /// <summary>
        /// Returns <see langword="true"/> if <see cref="SetupScope.IsActive"/> is <see langword="false" />, 
        /// since it will records all invocations in that case.
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => !SetupScope.IsActive;

        /// <summary>
        /// Implements the tracking of invocations for the excuted invocations.
        /// </summary>
        public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next)
        {
            invocation.Target.AsMock().Invocations.Add(invocation);

            return next().Invoke(invocation, next);
        }
    }
}