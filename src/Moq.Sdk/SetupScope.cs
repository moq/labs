﻿using System;
using System.Threading;

namespace Moq
{
    /// <summary>
    /// Marks the mock as being set up, meaning 
    /// invocation tracking and strict behavior 
    /// will be suspended until the scope is 
    /// is disposed.
    /// </summary>
    /// <devdoc>
    /// We make an exception with the namespace for this 
    /// type, since it's intended to be used by users 
    /// quite regularly, unless they are leveraging the 
    /// <c>using static Moq.Syntax;</c> which allows the 
    /// simpler <c>using (Setup()) { ... }</c> syntax.
    /// </devdoc>
    public class SetupScope : IDisposable
    {
        private static readonly AsyncLocal<bool?> setup = new AsyncLocal<bool?>();

        /// <summary>
        /// Initializes the setup scope.
        /// </summary>
        public SetupScope() => setup.Value = true;

        /// <summary>
        /// Whether there is an active setup scope in the running 
        /// execution context.
        /// </summary>
        public static bool IsActive => setup.Value == true;

        /// <summary>
        /// Disposes the scope, setting <see cref="IsActive"/> 
        /// back to <see langword="false"/>.
        /// </summary>
        public void Dispose() => setup.Value = null;
    }
}