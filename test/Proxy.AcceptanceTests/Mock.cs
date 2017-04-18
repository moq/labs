using Moq.Proxy;

public static class Mock
{
    // NOTE: for the proxy generator acceptance tests, we only care that we can 
    // generate proxies that compile without errors only, not that they can 
    // be constructed and used, so we can avoid taking more deps than necessary 
    // by just returning null from this method invocation. 
    // What matters to the generator is the presence of the [ProxyGenerator] 
    // attribute only, and the generic type parameters.
    [ProxyGenerator]
    public static T Of<T>() => default(T);
}