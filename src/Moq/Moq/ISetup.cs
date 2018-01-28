using System;

namespace Moq
{
    /// <summary>
    /// Extension point interface for extension methods 
    /// that act on void methods via <see cref="SetupExtension.Setup{T}(T, System.Action{T})"/>.
    /// </summary>
    public interface ISetup : IFluentInterface
    {
    }

    /// <summary>
    /// Extension point interface for extension methods 
    /// that act on delegates via <see cref="SetupExtension.Setup{TDelegate}(TDelegate)"/>.
    /// </summary>
    public interface ISetup<TDelegate> : IFluentInterface
    {
        /// <summary>
        /// The delegate passed to the <see cref="SetupExtension.Setup{TDelegate}(TDelegate)"/>
        /// call.
        /// </summary>
        Delegate Delegate { get; }
    }
}
