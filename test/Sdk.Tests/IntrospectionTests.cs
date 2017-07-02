using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Proxies;

namespace Tests
{
    public class IntrospectionTests
    {
        [Fact]
        public void CanIntrospectMock()
        {
            var mock = new ICalculatorProxy();

            var info = ((Moq.Sdk.IMocked)mock).Mock;

            //info.BehaviorFor()

        }
    }
}
