using System;

namespace ManualProxies
{
    public interface ITypeGetter
    {
        Type GetType(string assembly, string name);
    }
}
