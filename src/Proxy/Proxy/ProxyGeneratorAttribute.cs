using System;

namespace Moq.Proxy
{
    /// <summary>
    /// Annotates a method that is a factory for proxies, so that a 
    /// compile-time generator can generate them ahead of time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ProxyGeneratorAttribute : Attribute
    {
    }
}
