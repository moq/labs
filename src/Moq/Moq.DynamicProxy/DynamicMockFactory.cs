using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using Castle.DynamicProxy;
using Stunts;
using Stunts.Sdk;

namespace Moq.Sdk
{
    /// <summary>
    /// Provides an <see cref="IMockFactory"/> that creates types at run-time using Castle DynamicProxy.
    /// </summary>
    public class DynamicMockFactory : DynamicStuntFactory, IMockFactory
    {
        /// <inheritdoc />
        public object CreateMock(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
            => CreateStunt(mocksAssembly, baseType, implementedInterfaces, constructorArguments);
            
        /// <summary>
        /// Creates the mock proxy.
        /// </summary>
        protected override object CreateProxy(Type baseType, Type[] implementedInterfaces, object[] constructorArguments, bool notImplemented)
        {
            if (!implementedInterfaces.Contains(typeof(IMocked)))
            {
                var fixedInterfaces = new Type[implementedInterfaces.Length + 1];
                fixedInterfaces[0] = typeof(IMocked);
                implementedInterfaces.CopyTo(fixedInterfaces, 1);
                implementedInterfaces = fixedInterfaces;
            }

            var mocked = (IMocked)Generator.CreateClassProxy(baseType, implementedInterfaces, Options, constructorArguments, new DynamicMockInterceptor(notImplemented));

            // Save for cloning purposes. We opened a generated proxy from DP to figure out the ctor signature it creates.
            // The lazy-calculated value allows us to provide a new interceptor for every retrieval. 
            // Add first-class support in statebag for this pattern of either Func<T> for values, or 
            // Lazy<T>, since both could be quite useful for expensive state that may be needed lazily.
            mocked.Mock.State.Set(".ctor", () => new object[] { new IInterceptor[] { new DynamicMockInterceptor(notImplemented) } }.Concat(constructorArguments).ToArray());

            return mocked;
        }

        class DynamicMockInterceptor : DynamicStuntInterceptor
        {
            IMock mock;

            public DynamicMockInterceptor(bool notImplemented) : base(notImplemented) { }

            public override void Intercept(IInvocation invocation)
            {
                if (invocation.Method.DeclaringType == typeof(IMocked))
                {
                    invocation.ReturnValue = LazyInitializer.EnsureInitialized(ref mock, () => new DefaultMock((IStunt)invocation.Proxy));
                    return;
                }

                base.Intercept(invocation);
            }
        }
    }
}
