using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Proxies;
using Moq.Proxy;

namespace Moq.Sdk.Tests
{
    public class IntrospectionTests
    {
        [Fact]
        public void CanIntrospectMock()
        {
            var proxy = IntrospectionTests.Create<ICalculator>();
            var mock = new ICalculatorProxy();
           
            var info = ((IMocked)mock).Mock;

            //info.BehaviorFor
            //info.BehaviorFor()

        }

        [ProxyGenerator]
        public static T Create<T>() => default(T);
    }
}
