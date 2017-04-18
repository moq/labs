using System;

namespace Moq.ProxyDiscovererTests
{
    public class Tests
    {
        public void WhenMockingFormatterThenCanInvokeIt()
        {
            var mock1 = Mock.Of<ICustomFormatter>();
            var mock2 = Mock.Of<ICustomFormatter>();

            var result1 = mock1.Format("Hello {0}", "World", null);
            var result2 = mock2.Format("Hello {0}", "World", null);
        }
    }
}
