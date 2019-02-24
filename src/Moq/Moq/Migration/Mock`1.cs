using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Moq
{
    /// <summary>Obsolete</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Mock<T>
    {
        /// <summary>Obsolete</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Mock() => throw new NotSupportedException();

        /// <summary>Obsolete</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Mock(MockBehavior behavior) => throw new NotSupportedException();

        /// <summary>Obsolete</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Mock(params object[] args) => throw new NotSupportedException();

        /// <summary>Obsolete</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Mock(MockBehavior behavior, params object[] args) => throw new NotSupportedException();
    }
}