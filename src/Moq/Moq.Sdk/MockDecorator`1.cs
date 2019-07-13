using System.Collections.Generic;
using System.Collections.ObjectModel;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Decorator implementation over an <see cref="IMock"/>.
    /// </summary>
    public abstract class MockDecorator<T> : MockDecorator, IMock<T>
    {
        readonly IMock<T> mock;

        protected MockDecorator(IMock<T> mock) : base(mock) => this.mock = mock;

        /// <summary>
        /// See <see cref="IMock{T}.Object"/>.
        /// </summary>
        public new virtual T Object => mock.Object;
    }
}
