using System.Reflection;
using System;

namespace Moq.Proxy
{
    public class MethodCall : IMethodCall
    {
        public MethodCall(MethodInfo method, object instance, object[] arguments)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            InArgs = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        public object[] InArgs { get; private set; }

        public object Instance { get; private set; }

        public MethodInfo Method { get; private set; }
    }
}