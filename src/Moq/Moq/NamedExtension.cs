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
            var mock = (target is IMocked mocked) ?
                mocked.Mock : throw new ArgumentException(Resources.TargetNotMock);

            mock.State.Set("Name", name);
            return target;
        }
    }
}