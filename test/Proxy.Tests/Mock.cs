using Moq.Proxy;

public static class Mock
{
    [ProxyGenerator]
    public static T Of<T>() => default(T);
}