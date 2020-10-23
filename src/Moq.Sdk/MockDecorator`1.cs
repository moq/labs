namespace Moq.Sdk
{
    /// <summary>
    /// Decorator implementation over an <see cref="IMock"/>.
    /// </summary>
    public abstract class MockDecorator<T> : MockDecorator, IMock<T> where T : class
    {
        private readonly IMock<T> mock;

        /// <summary>
        /// Initializes the decorator with the given underlying <see cref="IMock{T}"/> 
        /// to use as default pass-through.
        /// </summary>
        protected MockDecorator(IMock<T> mock) : base(mock) => this.mock = mock;

        /// <summary>
        /// See <see cref="IMock{T}.Object"/>.
        /// </summary>
        public new virtual T Object => mock.Object;
    }
}
