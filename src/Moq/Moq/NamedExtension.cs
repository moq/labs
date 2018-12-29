using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Moq.Properties;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Extension for naming mocks.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class NamedExtension
    {
        /// <summary>
        /// Names the mock.
        /// </summary>
        public static T Named<T>(this T target, string name)
        {
            target.AsMock().State.Set("Name", name);
            return target;
        }
    }
}